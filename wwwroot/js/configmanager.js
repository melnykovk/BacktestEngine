const startBtn = document.getElementById("startBacktest");
const resultBlock = document.getElementById("result");
const strategyContainer = document.getElementById("strategy-params");
const rulesContainer = document.getElementById("rules-params");
const paramInputs = document.querySelectorAll(".param-block input");
startBtn.addEventListener("click", async () => {
    try {

        const request = getDataConfig();

        resultBlock.textContent = "Loading...";
        console.log("Start backtest clicked");
        const response = await fetch("/api/binance/grid-check", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!response.ok) {
            const errorText = await response.text();
            resultBlock.textContent = `HTTP ${response.status}\n${errorText}`;
            return;
        }

        const data = await response.json();

        // resultBlock.textContent = JSON.stringify(data, null, 2);
        document.getElementById("result").textContent =
            JSON.stringify(data, null, 2);
    }
    catch (error) {
        resultBlock.textContent = `Error: ${error.message}`;
    }
});

document.addEventListener('DOMContentLoaded', function () {
    createParamsConfig();

    // insert parameter blocks into the page
    [GridStep, LevelsPerSide, FeeRate, OrderSize].forEach(function (config) {
        strategyContainer.insertAdjacentHTML('beforeend', createParamBlock(config));
    });

    // Switching between modes
    document.querySelectorAll('.param-block').forEach(function (block) {
        block.querySelectorAll('input[type="radio"]').forEach(function (radio) {
            radio.addEventListener('change', function () {
                block.querySelectorAll('.mode-panel').forEach(function (panel) {
                    panel.hidden = panel.dataset.mode !== radio.value;
                });
            });
        });
    });
});


document.addEventListener('DOMContentLoaded', function () {
    createRulesConfig();

    // insert parameter blocks into the page
    [Interval, Threshold, Multiplier].forEach(function (config) {
        rulesContainer.insertAdjacentHTML('beforeend', createRulesBlock(config));
    });

    // Switching between modes
    document.querySelectorAll('.rules-block').forEach(function (block) {
        block.querySelectorAll('input[type="radio"]').forEach(function (radio) {
            radio.addEventListener('change', function () {
                block.querySelectorAll('.mode-panel').forEach(function (panel) {
                    panel.hidden = panel.dataset.mode !== radio.value;
                });
            });
        });
    });
});


[strategyContainer, rulesContainer].forEach((container) => {
    container.addEventListener("input", (e) => {
        if (e.target.matches("input")) {
            updateStrategyRunsCount();
        }
    });
});
function getDataConfig() {
    const symbol = document.getElementById('symbol').value;
    const interval = document.getElementById('interval').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;

    const gridSteps = getValues("gridStep");
    const levelsPerSide = getValues("levelsPerSide");
    const feeRates = getValues("feeRate");
    const orderSizes = getValues("orderSize");

    const rangeData = {
        Symbol: symbol,
        Interval: interval,
        StartDate: startDate,
        EndDate: endDate
    };
    const BacktestBatchRequest = buildRuns(
        rangeData,
        gridSteps,
        feeRates,
        orderSizes,
        levelsPerSide,
        [Number(document.getElementById("startingQuote").value)]
    );
    return {
        rangeData: rangeData,
        runs: BacktestBatchRequest
    };
}


function runsStrategyCount() {
    const gridSteps = getValues("gridStep");
    const levelsPerSide = getValues("levelsPerSide");
    const feeRates = getValues("feeRate");
    const orderSizes = getValues("orderSize");
    const interval = getValues("interval");
    const threshold = getValues("threshold");
    const multiplier = getValues("multiplier");
    return gridSteps.length * levelsPerSide.length * feeRates.length * orderSizes.length * interval.length * threshold.length * multiplier.length;
}

function updateStrategyRunsCount() {
    const count = runsStrategyCount();
    document.getElementById("runsNumber").textContent = count;
}
function getValues(string) {
    const block = document.querySelector(`[data-param="${string}"]`);
    return getParamValues(block);
}



function buildRuns(rangeData, gridSteps, feeRates, orderSizeQuotes, levelsPerSide, startingQuotes) {
    const runs = [];
    let id = 1;

    for (const startingQuote of startingQuotes) {
        for (const gridStep of gridSteps) {
            for (const levels of levelsPerSide) {
                for (const feeRate of feeRates) {
                    for (const orderSizeQuote of orderSizeQuotes) {
                        runs.push({
                            id: String(id++),
                            rangeData: rangeData,
                            startingQuote: startingQuote,
                            gridConfig: {
                                levelsPerSide: levels,
                                step: gridStep,
                                orderSizeQuote: orderSizeQuote
                            },
                            feeRate: feeRate
                        });
                    }
                }
            }
        }
    }
    return runs;
}


function getParamValues(block) {
    const mode = block.querySelector('input[type="radio"]:checked');
    if (!mode) {
        throw new Error('No selected mode found');
    }
    if (mode.value === 'single') {
        return getSingleValue(block);
    }

    if (mode.value === 'range') {
        return getRangeValues(block);
    }

    if (mode.value === 'list') {
        return getListValues(block);
    }
    throw new Error(`Unknown mode: ${mode.value}`);
}


function getRangeValues(block) {
    const inputFrom = block.querySelector('input[data-role="from"]');
    const inputTo = block.querySelector('input[data-role="to"]');
    const inputStep = block.querySelector('input[data-role="step"]');

    if (!inputFrom || !inputTo || !inputStep) {
        throw new Error('Range inputs not found');
    }
    const from = inputFrom.valueAsNumber;
    const to = inputTo.valueAsNumber;
    const step = inputStep.valueAsNumber;
    const values = [];
    if (step <= 0) {
        throw new Error('Step must be > 0');
    }
    if (from > to) {
        throw new Error('From cannot be greater than To');
    }
    for (let current = from; current <= to + Number.EPSILON; current += step) {
        values.push(Number(current.toFixed(10)));
    }
    return values;
}
function getListValues(block) {
    const input = block.querySelector('input[type="text"]');
    if (!input) {
        throw new Error('List input not found');
    }
    return input.value
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0)
        .map(s => Number(s))
        .filter(s => !isNaN(s));
}
function getSingleValue(block) {
    const input = block.querySelector('input[type="number"]');
    if (!input) {
        throw new Error('Single input not found');
    }
    return [input.valueAsNumber];
}



function createParamBlock(config) {
    return `
                <div class="param-block" data-param="${config.name}">
                        <h3>${config.label}</h3>

                        <div class="mode-switch">
                            <label><input type="radio" name="${config.name}Mode" value="single" checked> Single</label>
                            <label><input type="radio" name="${config.name}Mode" value="range"> Range</label>
                            <label><input type="radio" name="${config.name}Mode" value="list"> List</label>
                        </div>

                        <div class="mode-panel" data-mode="single">
                            <div class="range-row">
                                <label>Value</label>
                                <input type="number" step="${config.step}" value="${config.value}">
                            </div>
                        </div>

                        <div class="mode-panel" data-mode="range" hidden>
                            <div class="range-row">
                                <label>From</label>
                                <input data-role="from" type="number" step="${config.step}" value="${config.from}">
                            </div>

                            <div class="range-row">
                                <label>To</label>
                                <input data-role="to" type="number" step="${config.step}" value="${config.to}">
                            </div>

                            <div class="range-row">
                                <label>Step</label>
                                <input data-role="step" type="number" step="${config.step}" value="${config.step}">
                            </div>
                        </div>

                        <div class="mode-panel" data-mode="list" hidden>
                            <div class="range-row">
                                <label>Values</label>
                                <input type="text" value="${config.values.join(',')}">
                            </div>
                        </div>
                
            </div>
    `;
}

function createRulesBlock(config) {
    return `
                <div class="rules-block" data-param="${config.name}">
                        <h3>${config.label}</h3>

                        <div class="mode-switch">
                            <label><input type="radio" name="${config.name}Mode" value="single" checked> Single</label>
                            <label><input type="radio" name="${config.name}Mode" value="range"> Range</label>
                            <label><input type="radio" name="${config.name}Mode" value="list"> List</label>
                        </div>

                        <div class="mode-panel" data-mode="single">
                            <div class="range-row">
                                <label>Value</label>
                                <input type="number" step="${config.step}" value="${config.value}">
                            </div>
                        </div>

                        <div class="mode-panel" data-mode="range" hidden>
                            <div class="range-row">
                                <label>From</label>
                                <input data-role="from" type="number" step="${config.step}" value="${config.from}">
                            </div>

                            <div class="range-row">
                                <label>To</label>
                                <input data-role="to" type="number" step="${config.step}" value="${config.to}">
                            </div>

                            <div class="range-row">
                                <label>Step</label>
                                <input data-role="step" type="number" step="${config.step}" value="${config.step}">
                            </div>
                        </div>

                        <div class="mode-panel" data-mode="list" hidden>
                            <div class="range-row">
                                <label>Values</label>
                                <input type="text" value="${config.values.join(',')}">
                            </div>
                        </div>
                
            </div>
    `;
}

function loadStrategyParams(name, label, step, value, from, to, values) {
    var config = {
        name: name,
        label: label,
        step: step,
        value: value,
        from: from,
        to: to,
        values: values
    }
    return config;
}
function createParamsConfig() {
    GridStep = loadStrategyParams("gridStep", "Grid Step", 0.002, 0.008, 0.005, 0.019, [0.004, 0.009, 0.014]);
    LevelsPerSide = loadStrategyParams("levelsPerSide", "Levels Per Side", 5, 25, 20, 50, [10, 20, 30]);
    FeeRate = loadStrategyParams("feeRate", "Fee Rate", 0.0001, 0.0004, 0.0002, 0.0005, [0.0001, 0.0002, 0.0003]);
    OrderSize = loadStrategyParams("orderSize", "Order Size", 5, 50, 20, 50, [10, 25, 50]);
}

function createRulesConfig() {
    Interval = loadStrategyParams("interval", "Interval", 0.002, 0.008, 0.005, 0.019, [0.004, 0.009, 0.014]);
    Threshold = loadStrategyParams("threshold", "Threshold", 5, 25, 20, 50, [10, 20, 30]);
    Multiplier = loadStrategyParams("multiplier", "Multiplier", 0.0001, 0.0004, 0.0002, 0.0005, [0.0001, 0.0002, 0.0003]);
}
