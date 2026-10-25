import type {
    DbCandleSummaryResponseDto,
    DbDatasetDeleteRequestDto,
    DbDatasetDeleteResponseDto,
    DbUploadKlinesRequestDto,
    DbUploadKlinesResponseDto
} from "./dbmanagerdto";

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


// delete the kline dataset for a given symbol and interval
export async function deleteDbKlines(dto: DbDatasetDeleteRequestDto): Promise<ApiResult<DbDatasetDeleteResponseDto>>{
    return await requestWithStatus<DbDatasetDeleteResponseDto, DbDatasetDeleteRequestDto>
    ("DELETE", "/api/binance/delete-db-klines", dto);
}

// get the list of datasets in the database
export async function getKlineDatasets() : Promise<DbCandleSummaryResponseDto[]>{
    return await request<DbCandleSummaryResponseDto[], undefined>
    ("GET", "/api/binance/get-db-klines");
}

// upload a dataset for a given symbol and interval
export async function uploadKlineDataset(dto: DbUploadKlinesRequestDto, signal?: AbortSignal): Promise<ApiResult<DbUploadKlinesResponseDto>>{
    return await requestWithStatus<DbUploadKlinesResponseDto, DbUploadKlinesRequestDto>
    ("POST", "/api/binance/upload-klines-dataset", dto, signal);
}