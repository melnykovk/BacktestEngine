export type ParamMode = "single" | "range" | "list";


export interface ParamConfig {
    name: string;
    label: string;
    step: number;
    value: number;
    from: number;
    to: number;
    values: number[];
}

export interface ParamState {
    mode: ParamMode;
    value: string;
    from: string;
    to: string;
    step: string;
    list: string;
}

export function initParamState(c: ParamConfig): ParamState {
    return {
        mode: "single",
        value: String(c.value),
        from: String(c.from),
        to: String(c.to),
        step: String(c.step),
        list: c.values.join(",")
    };
}
function toNumber(raw: string, field: string): number {
    const n = Number(raw);
    if (raw.trim() === "" || Number.isNaN(n)) {
        throw new Error(`${field} must be a number`);
    }
    return n;
}

function getRangeValues({ from, to, step }: ParamState): number[] {
    const start = toNumber(from, "From");
    const end = toNumber(to, "To");
    const inc = toNumber(step, "Step");

    if (inc <= 0) throw new Error("Step must be > 0");
    if (start > end) throw new Error("From cannot be greater than To");
 
    const values: number[] = [];

    for (let current = start; current <= end; current = Number((current+inc).toFixed(6))) {
        values.push(current);
    }
    return values;
}

function getListValues(list: string): number[] {
    return list
        .split(",")
        .map(s => s.trim())
        .filter(s => s.length > 0)
        .map(Number)
        .filter(n => !Number.isNaN(n));
}

export function getParamValues(state: ParamState): number[] {
    switch (state.mode) {
        case "single": return [toNumber(state.value, "Value")];
        case "range":  return getRangeValues(state);
        case "list":   return getListValues(state.list);
    }
}

export const STRATEGY_PARAMS: ParamConfig[] = [
    { name: "gridStep",      label: "Grid Step",       step: 0.002,  value: 0.008,  from: 0.005,  to: 0.019, values: [0.004, 0.009, 0.014] },
    { name: "levelsPerSide", label: "Levels Per Side", step: 5,      value: 25,     from: 20,     to: 50,    values: [10, 20, 30] },
    { name: "feeRate",       label: "Fee Rate",        step: 0.0001, value: 0.0004, from: 0.0002, to: 0.0005, values: [0.0001, 0.0002, 0.0003] },
    { name: "orderSize",     label: "Order Size",      step: 5,      value: 50,     from: 20,     to: 50,    values: [10, 25, 50] }
];

export const RULES_PARAMS: ParamConfig[] = [
    { name: "interval",   label: "Interval",   step: 0.002,  value: 0.008,  from: 0.005,  to: 0.019, values: [0.004, 0.009, 0.014] },
    { name: "threshold",  label: "Threshold",  step: 5,      value: 25,     from: 20,     to: 50,    values: [10, 20, 30] },
    { name: "multiplier", label: "Multiplier", step: 0.0001, value: 0.0004, from: 0.0002, to: 0.0005, values: [0.0001, 0.0002, 0.0003] }
];