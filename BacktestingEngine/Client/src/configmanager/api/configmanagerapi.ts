import type { BacktestBatchRequestDto } from "./configmanagerdto";
import type { BacktestRunResultResponseDto } from "./configmanagerdto";
import type { BacktestResultDeleteRequestDto } from "./configmanagerdto";
import type { BacktestResultDeleteResponseDto } from "./configmanagerdto";

export interface ApiResult<T> {
    status: number;
    ok: boolean;
    body: T | string | null;
}




// generic request function for making API calls
export async function request<TResponse, TBody = undefined>(
    method: RequestInit["method"],
    url: string,
    body?: TBody
  ): Promise<TResponse> {

    const options: RequestInit = { method };

    if (body !== undefined) {
      options.headers = { "Content-Type": "application/json" };
      options.body = JSON.stringify(body);
    }
    const response = await fetch(url, options);

    if(!response.ok){
        throw new Error(`HTTP ${response.status}: ${await response.text()}`)
    }
    return await response.json() as TResponse;
}

export async function requestWithStatus<TResponse, TBody = undefined>(
    method: RequestInit["method"],
    url: string,
    body?: TBody,
    signal?: AbortSignal
): Promise<ApiResult<TResponse>> {

    const options: RequestInit = { method, signal };

    if (body !== undefined) {
        options.headers = { "Content-Type": "application/json" };
        options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);
    const text = await response.text();

    let parsed: TResponse | string | null = null;
    if (text) {
        try { parsed = JSON.parse(text) as TResponse; }
        catch { parsed = text; }
    }

    return { status: response.status, ok: response.ok, body: parsed };
}

export async function runBacktest(dto: BacktestBatchRequestDto, signal?: AbortSignal): Promise<ApiResult<BacktestRunResultResponseDto>>{
    return await requestWithStatus<BacktestRunResultResponseDto, BacktestBatchRequestDto>
        ("POST", "/api/binance/backtest-run", dto, signal);
}
export async function getBacktestDatasets() : Promise<BacktestRunResultResponseDto[]>{
    return await request<BacktestRunResultResponseDto[], undefined>
    ("GET", "/api/binance/get-db-backtest-results");
}






export async function deleteBacktestResult(dto: BacktestResultDeleteRequestDto, signal?: AbortSignal) : Promise<ApiResult<BacktestResultDeleteResponseDto>>{
    return await requestWithStatus<BacktestResultDeleteResponseDto, BacktestResultDeleteRequestDto>
    ("DELETE", "/api/binance/delete-batchresult", dto, signal);
}
export async function getBacktestStats(id: string, signal?: AbortSignal): Promise<Blob> {
    const response = await fetch(
        `/api/binance/download-backtest-result?id=${encodeURIComponent(id)}`,
        {
            method: "GET",
            signal 
        }
        );

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${await response.text()}`);
    }

    return await response.blob();
}
