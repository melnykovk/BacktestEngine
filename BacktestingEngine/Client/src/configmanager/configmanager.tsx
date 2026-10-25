
import { useState } from "react";
import { useRef } from "react";
import { useMemo } from "react";
import { useEffect } from "react";

import "../css/base/layout.css"
import "../css/base/result.css";
import styles from "./css/configmanager.module.css";

import { ParamBlock } from "./paramblock.tsx";
import { RULES_PARAMS, STRATEGY_PARAMS } from "./hooks/useconfigmanager.ts";
import { getParamValues, initParamState, type ParamConfig, type ParamState } from "./hooks/useconfigmanager";
import { runBacktest } from "./api/configmanagerapi.ts";
import { getBacktestDatasets } from "./api/configmanagerapi.ts";
import { buildRuns } from "./hooks/buildRuns";
import { deleteBacktestResult } from "./api/configmanagerapi.ts";
import { getBacktestStats } from "./api/configmanagerapi.ts";
import type { BacktestBatchRequestDto } from "./api/configmanagerdto.ts";
import type { RangeBacktestData } from "./api/configmanagerdto.ts";
import type { BacktestRunResultResponseDto } from "./api/configmanagerdto.ts";
import type { BacktestResultDeleteRequestDto } from "./api/configmanagerdto.ts";


export interface ApiResult<T> {
    status: number;
    ok: boolean;
    body: T | string | null;
}
type ParamsState = Record<string, ParamState>;
function initParams(configs: ParamConfig[]): ParamsState {
    return Object.fromEntries(configs.map(c => [c.name, initParamState(c)]));
}


export function ConfigManager() {

    const [data, setData] = useState({
        name: "",
        symbol: "BTCUSDT",
        interval: "1m",
        startDate: "",
        endDate: "",
        startingQuote: "1000"
    });
    const [strategy, setStrategy] = useState(() => initParams(STRATEGY_PARAMS));
    const [rules, setRules] = useState(() => initParams(RULES_PARAMS));
    const [isLoading, setIsLoading] = useState(true);

    const [datasets, setDatasets] = useState<BacktestRunResultResponseDto[]>([]);
    const [result, setResult] = useState<ApiResult<unknown> | null>(null);
    const [isRunning, setIsRunning] = useState(false);
    const abortRef = useRef<AbortController | null>(null);

    const dialogRef = useRef<HTMLDialogElement>(null);
    const downloadDialogReg = useRef<HTMLDialogElement>(null);
    const [datasetToDelete, setDatasetToDelete] = useState<BacktestResultDeleteRequestDto | null>(null);
    const [backtestDownload, setBacktestDownload] = useState<BacktestResultDeleteRequestDto | null> (null);
    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [downloadError, setDownloadError] = useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [isDownloading, setIsDownloading] = useState(false);


    function askDelete(dataset: BacktestResultDeleteRequestDto){
                setDatasetToDelete(dataset);
                setDeleteError(null);
                dialogRef.current?.showModal();
            };


    function askDownload(dataset: BacktestResultDeleteRequestDto){
                setBacktestDownload(dataset);
                setDownloadError(null);
                downloadDialogReg.current?.showModal();
            };

    useEffect(() => {loadDatasets();}, []);
    async function loadDatasets(){
                setIsLoading(true);
                try {
                    const data = await getBacktestDatasets();
                    setDatasets(data);
                }
                finally{
                    setIsLoading(false);
                }
            };


    async function confirmDelete(){
        if(!datasetToDelete) return
        const controller = new AbortController();
        abortRef.current = controller;
        setIsDeleting(true);
        setDeleteError(null);
        
        try{
            const response = await deleteBacktestResult(datasetToDelete, controller.signal)
            console.log("Delete response:", response)
            setResult(response);
            dialogRef.current?.close();
            loadDatasets();
        }catch(error){
            setDeleteError("Failed to delete");
        }finally {
            abortRef.current = null;
            setIsDeleting(false);
        }
    };


    async function confirmDownload(){
        if(!backtestDownload) return
        const controller = new AbortController();
        abortRef.current = controller;
        setIsDownloading(true);
        setDownloadError(null);
        
        try {
        const blob = await getBacktestStats(backtestDownload.id, controller.signal);

        const url = URL.createObjectURL(blob);
        const a = Object.assign(document.createElement("a"), {
            href: url,
            download: `backtest-results-${backtestDownload.id}.csv`
        });

        a.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);

        downloadDialogReg.current?.close();
    } catch (error) {
        console.error("Download error:", error);
        setDownloadError("Failed to download");
    } finally {
        abortRef.current = null;
        setIsDownloading(false);
    }
    };

    const runsCount = useMemo(() => {
        try {
            const all = { ...strategy, ...rules };
            return [...STRATEGY_PARAMS, ...RULES_PARAMS]
                .reduce((acc, c) => acc * getParamValues(all[c.name]).length, 1);
        } catch {
            return 0; // в каком-то поле некорректное значение
        }
    }, [strategy, rules]);

    function buildPayload(): BacktestBatchRequestDto {
        const rangeData: RangeBacktestData = {
            name: data.name,
            symbol: data.symbol,
            interval: data.interval,
            startDate: data.startDate,
            endDate: data.endDate
        };

        const runs = buildRuns({
            rangeData,
            startingQuotes: [Number(data.startingQuote)],
            gridSteps: getParamValues(strategy.gridStep),
            levelsPerSide: getParamValues(strategy.levelsPerSide),
            feeRates: getParamValues(strategy.feeRate),
            orderSizes: getParamValues(strategy.orderSize)
        });

        return { rangeData, runs };
    }   

    async function handleStart() {
        if (abortRef.current) {
            abortRef.current.abort();
            return;
        }

        const controller = new AbortController();
        abortRef.current = controller;
        setIsRunning(true);

        try {
            const payload = buildPayload();

            const response = await runBacktest(payload, controller.signal);
            setResult(response);
            if(response.ok){
            loadDatasets();
            }
        } catch (error) {
            const cancelled = error instanceof DOMException && error.name === "AbortError";
            setResult({
                status: 0,
                ok: false,
                body: cancelled ? "Operation cancelled by user" : "Network error",
            });
        } finally {
            abortRef.current = null;
            setIsRunning(false);
        }
    }

    return (
        <>
            <main className={styles["workspace"]}>
                <h1>Workspace</h1>



                <div className={styles["table-wrapper"]}>
                        <table className={styles["data-table"]}>
                            <thead>
                                <tr>
                                    <th>Id</th>
                                    <th>Name</th>
                                    <th>Symbol</th>
                                    <th>Interval</th>
                                    <th>Start Date</th>
                                    <th>End Date</th>
                                    <th>Runs</th>
                                    <th>Durations</th>
                                    <th>Created UTC</th>
                                    <th>Action</th>
                                </tr>
                            </thead>

                            <tbody>
                                {
                                    [...datasets].map(dataset => (
                                        <tr key={dataset.id}>
                                            <td
                                            title={`${dataset.id}\nClick to copy`}
                                            className={styles["id-cell"]}
                                            onClick={() => navigator.clipboard.writeText(dataset.id)}
                                                >
                                            {dataset.id.slice(0, 5).toUpperCase()}
                                        </td>
                                            <td>{dataset.name}</td>
                                            <td>{dataset.symbol.toUpperCase()}</td>
                                            <td>{dataset.interval}</td>
                                            <td>{new Date (dataset.startDate).toISOString().slice(0, 10).replace("T", " ")}</td>
                                            <td>{new Date (dataset.endDate).toISOString().slice(0, 10).replace("T", " ")}</td>
                                            <td>{dataset.runsCount}</td>
                                            <td>{dataset.executionTime}s</td>
                                            <td>{new Date (dataset.createdAtUtc).toISOString().slice(0, 16).replace("T", " ")}</td>
                                            <td>
                                                <button
                                                    className={styles["download-button"]}
                                                    onClick={() => askDownload(dataset)}
                                                    >
                                                    ↓
                                                </button>
                                                
                                                <button
                                                    className={styles["delete-button"]}
                                                    onClick={() => askDelete(dataset)}
                                                    >
                                                    ✖
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                            </tbody>




                        </table>
                    </div>

                <div className={styles["result-block"]}>
                    <h2>Result</h2>
                    <div id="result">
                        {result && (
                                <pre>
                                    HTTP {result.status}{"\n"}
                                    {JSON.stringify(result.body, null, 2)}
                                </pre>)} 

                    </div>
                </div>
            </main>


                                <dialog ref={dialogRef} className={styles["confirm-dialog"]}>
                                            <h3>Confirm deletion</h3>
                                            <p>Delete {datasetToDelete?.id.slice(0,8).toUpperCase()} dataset ?</p>

                                            {deleteError && <p className={styles["error-text"]}>{deleteError}</p>}

                                                <div className={styles["confirm-actions"]}>
                                                    <button 
                                                        className={styles["btn-danger"]}
                                                        onClick={confirmDelete}
                                                        disabled = {isDeleting}>
                                                        {isDeleting? <span className={styles["spinner"]}> </span> : "Delete"}
                                                    </button>

                                                    <button className={styles["btn-cancel"]}
                                                        onClick={() => dialogRef.current?.close()}
                                                        disabled={isDeleting}>
                                                        Cancel
                                                        </button>
                                                </div>
                                        </dialog>


                                        <dialog ref={downloadDialogReg} className={styles["confirm-dialog"]}>
                                            <h3>Confirm download</h3>
                                            <p>Download {backtestDownload?.id.slice(0,8).toUpperCase()} backtest ?</p>

                                            {downloadError && <p className={styles["error-text"]}>{downloadError}</p>}

                                                <div className={styles["confirm-actions"]}>
                                                    <button 
                                                        className={styles["btn-download"]}
                                                        onClick={confirmDownload}
                                                        disabled = {isDownloading}>
                                                        {isDownloading? <span className={styles["spinner"]}> </span> : "Download"}
                                                    </button>

                                                    <button className={styles["btn-cancel"]}
                                                        onClick={() => downloadDialogReg.current?.close()}
                                                        disabled={isDownloading}>
                                                        Cancel
                                                        </button>
                                                </div>
                                        </dialog>


            <aside className={styles["config-panel"]}>
                <h2>Backtest Config</h2>
                <div className={styles["structure-block"]}>
                    <h3>Data Config</h3>

                    <label htmlFor="config-name">Name Config</label>
                    <input id="config-name" type="string" value = {data.name}
                    onChange={e => setData({...data, name: e.target.value})}
                    placeholder="e.g. Deep grid check (not necessarily)"></input>


                    <label htmlFor="symbol">Symbol</label>
                    <input id="symbol" type="string" value={data.symbol}
                        onChange={e => setData({ ...data, symbol: e.target.value.toUpperCase() })}/>

                    

                    <label htmlFor="interval">Interval</label>
                    <select id="interval"
                    onChange={e => setData({...data, interval: e.target.value})}>
                        <option value="1m">1m</option>
                        <option value="5m">5m</option>
                        <option value="15m">15m</option>
                        <option value="30m">30m</option>
                        <option value="1h">1h</option>
                        <option value="4h">4h</option>
                        <option value="12h">12h</option>
                        <option value="1d">1d</option>
                        
                    </select>

                    <label htmlFor="startDate">Start Date</label>
                    <input id="startDate" type="date" value={data.startDate}
                        onChange={e => setData({ ...data, startDate: e.target.value })}/>
 
                    <label htmlFor="endDate">End Date</label>
                    <input id="endDate" type="date" value={data.endDate}
                        onChange={e => setData({ ...data, endDate: e.target.value })} />


                    

                    <h3>Strategy Config</h3>

                    <label htmlFor="startingQuote">Starting Balance</label>
                    <input id="startingQuote" type="number" step="500" value={data.startingQuote}
                           onChange={e => setData({ ...data, startingQuote: e.target.value })} />

                    {STRATEGY_PARAMS.map(c => (
                        <ParamBlock key={c.name} config={c} state={strategy[c.name]}
                                    onChange={next => setStrategy(s => ({ ...s, [c.name]: next }))} />
                    ))}

                    {/* <h3>Rules Config</h3>
                    {RULES_PARAMS.map(c => (
                        <ParamBlock key={c.name} config={c} state={rules[c.name]}
                                    onChange={next => setRules(s => ({ ...s, [c.name]: next }))} />
                    ))} */}

                    <div id="runsCount">
                        <span className={styles["runs-count"]}>Runs to execute: {" "} <span id="runsNumber">{runsCount}</span></span>
                    </div>

                    <button 
                    className={styles["action-button"]}
                    onClick={handleStart}>
                    {isRunning ? "Cancel" : "Start backtest"}
                    </button>
                </div>
            </aside>
        </>
)};