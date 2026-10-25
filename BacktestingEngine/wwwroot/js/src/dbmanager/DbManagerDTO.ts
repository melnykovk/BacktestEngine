

// DTOs for database operations


interface DbCandleSummaryResponseDto {       // Response DTO for getting kline dataset
    symbol: string;
    interval: string;
    firstOpenTimeMs: number;
    lastOpenTimeMs: number;
    candlesCount: number;
}


interface DbDatasetDeleteRequestDto {        // Request DTO for deleting a dataset
    symbol: string;
    interval: string;
}
interface DbDatasetDeleteResponseDto {       // Response DTO for deleting a dataset
    symbol: string;
    interval: string;
    deletedCount: number;
}


interface DbUploadKlinesRequestDto{          // Request DTO for uploading kline dataset from API
    symbol: string;
    interval: string;
    startDate: string;
    endDate: string;
}
interface DbUploadKlinesResponseDto{         // Response DTO for uploaded kline dataset from API
    symbol: string;
    interval: string;
    requestedRange: string;
    updatedExistingCandles: number;
    addedNewCandles: number;
    startDate: string;
    endDate: string;
    firstCandleTime: number;
    lastCandleTime: number;
}