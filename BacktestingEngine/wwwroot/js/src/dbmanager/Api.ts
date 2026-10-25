
// @ts-check

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


// delete the kline dataset for a given symbol and interval
export async function deleteDbKlines(dto: DbDatasetDeleteRequestDto): Promise<DbDatasetDeleteResponseDto>{
    return await request<DbDatasetDeleteResponseDto, DbDatasetDeleteRequestDto>
    ("DELETE", "/api/binance/delete-db-klines", dto);
}

// get the list of datasets in the database
export async function getKlineDatasets() : Promise<DbCandleSummaryResponseDto[]>{
    return await request<DbCandleSummaryResponseDto[], undefined>
    ("GET", "/api/binance/get-db-klines");
}

// upload a dataset for a given symbol and interval
export async function uploadKlineDataset(dto: DbUploadKlinesRequestDto): Promise<DbUploadKlinesResponseDto>{
    return await request<DbUploadKlinesResponseDto, DbUploadKlinesRequestDto>
    ("POST", "/api/binance/upload-klines-dataset", dto);
}