import type { BacktestRunRequestDto, RangeBacktestData } from "../api/configmanagerdto";
interface RangeData {
    Symbol: string;
    Interval: string;
    StartDate: string;
    EndDate: string;
}

interface BuildRunsInput {
    rangeData: RangeBacktestData;
    startingQuotes: number[];
    gridSteps: number[];
    levelsPerSide: number[];
    feeRates: number[];
    orderSizes: number[];
}

export function buildRuns(input: BuildRunsInput): BacktestRunRequestDto[] {
    const { rangeData, startingQuotes, gridSteps, levelsPerSide, feeRates, orderSizes } = input;
    const runs: BacktestRunRequestDto[] = [];
    let id = 1;

    for (const startingQuote of startingQuotes)
        for (const step of gridSteps)
            for (const levels of levelsPerSide)
                for (const feeRate of feeRates)
                    for (const orderSizeQuote of orderSizes)
                        runs.push({
                            id: String(id++),
                            rangeData,
                            startingQuote,
                            gridConfig: { levelsPerSide: levels, step, orderSizeQuote },
                            feeRate
                        });

    return runs;
}