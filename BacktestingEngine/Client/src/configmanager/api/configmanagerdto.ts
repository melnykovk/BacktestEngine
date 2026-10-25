export interface BacktestBatchRequestDto{
    rangeData: RangeBacktestData;
    runs: BacktestRunRequestDto[];
}
export interface BacktestRunRequestDto{
    id: string;  
    rangeData: RangeBacktestData;
    startingQuote: number;
    gridConfig: GridConfig;
    feeRate: number;
}
export interface RangeBacktestData{
    name: string;
    symbol: string;
    interval: string;
    startDate: string;
    endDate: string;
}
export interface GridConfig {
    levelsPerSide: number;
    step: number;
    orderSizeQuote: number;
}

export interface BacktestRunResultResponseDto{
    id: string;
    name: string;
    symbol: string;
    interval: string;
    startDate: number;
    endDate: number;
    runsCount: number;
    executionTime: number;
    createdAtUtc: number;
}
export interface BacktestResultDeleteResponseDto{
    id: string;
    name: string;
    symbol: string;
    interval: string;
    startDate: number;
    endDate: number;
    runsCount: number;
}
export interface BacktestResultDeleteRequestDto{
    id: string;
}