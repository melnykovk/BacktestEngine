import { useDbManager } from "./hooks/usedbmanager";
import styles from "./css/dbmanager.module.css";
import "../css/base/layout.css"
import "../css/base/result.css";
import { KlineInterval} from "../Helpers/intervals";

export function DbManager(){
        const {
            datasets, datasetToDelete, result, dialogRef, isLoading, isUploading,
            isDeleting, deleteError, symbol, interval, startDate, endDate, askDelete, 
            confirmDelete, uploadDataset, setSymbol, setInterval, setStartDate,
            setEndDate, cancelUpload
            } = useDbManager();
    
    return (
        <>
            <main className={styles["workspace"]}>         
                    <h1>Database</h1>
                    {
                    isLoading && (<div className={styles["status-text"]}>
                        Loading database information...
                        </div>)
                    }
                    <div className={styles["table-wrapper"]}>
                        <table className={styles["data-table"]}>
                            <thead>
                                <tr>
                                    <th>Symbol</th>
                                    <th>Interval</th>
                                    <th>First Candle</th>
                                    <th>Last Candle</th>
                                    <th>Candles Count</th>
                                    <th>Delete</th>
                                </tr>
                            </thead>

                            <tbody>
                                {[...datasets]
                                    .sort((a, b) =>
                                        a.symbol.localeCompare(b.symbol) ||
                                        KlineInterval[a.interval as keyof typeof KlineInterval] -
                                        KlineInterval[b.interval as keyof typeof KlineInterval]
                                    )
                                    .map(dataset => (
                                        <tr key={`${dataset.symbol}-${dataset.interval}`}>
                                            <td>{dataset.symbol}</td>
                                            <td>{dataset.interval}</td>
                                            <td>{new Date(dataset.firstOpenTimeMs).toISOString().slice(0, 16).replace("T", " ")}</td>
                                            <td>{new Date(dataset.lastOpenTimeMs).toISOString().slice(0, 16).replace("T", " ")}</td>
                                            <td>{dataset.candlesCount}</td>
                                            <td>
                                                <button
                                                    className={styles["delete-button"]}
                                                    onClick={() => askDelete(dataset)}>
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
                                            <p>Delete {datasetToDelete?.symbol} {datasetToDelete?.interval} dataset ?</p>

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

                <aside className={styles["config-panel"]}>
                    <h2>Upload Candles</h2>
                    <h3>Data Config </h3>

                    <div className={styles["data-input"]}>
                        <label htmlFor="symbol">Symbol</label>
                        <input 
                        type="text" 
                        id="symbol" 
                        placeholder="Trading symbol (e.g. BTCUSDT)"
                        value = {symbol}
                        onChange={e => setSymbol(e.target.value)}
                        />
                    </div>

                    <div className={styles["data-input"]}>
                        <label htmlFor="interval">Interval</label>
                        <select 
                        id="interval"
                        value={interval}
                        onChange={e => setInterval(e.target.value)}
                        >
                            <option value="1m">1m</option>
                            <option value="5m">5m</option>
                            <option value="15m">15m</option>
                            <option value="30m">30m</option>
                            <option value="1h">1h</option>
                            <option value="4h">4h</option>
                            <option value="12h">12h</option>
                            <option value="1d">1d</option>
                        </select>
                    </div>

                    <div className={styles["data-input"]}>
                        <label htmlFor="startDate">Start Date</label>
                        <input
                        type="date"
                        id="startDate"
                        value={startDate}
                        onChange={e => setStartDate(e.target.value)}
                        />
                    </div>

                    <div className={styles["data-input"]}>
                        <label htmlFor="endDate">End Date</label>
                        <input 
                        type="date" 
                        id="endDate"
                        value={endDate}
                        onChange={e => setEndDate(e.target.value)}
                        />
                    </div>

                    <button 
                    id="loadDataButton" 
                    className={styles["action-button"]}
                    onClick={isUploading ?
                        cancelUpload : uploadDataset}>
                        {isUploading && <span className={styles["spinner"]}></span>}
                        <span>{isUploading ? "Cancel" : "Load Data"}</span>   
                    </button>

                </aside>  
        </>
    )
}