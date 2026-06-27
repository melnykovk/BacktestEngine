let uploadAbortController = null;
let isUploading = false;

document.addEventListener("DOMContentLoaded", () => {
    loadDatabaseInfo();

    const loadDataButton = document.getElementById("loadDataButton");

    loadDataButton.addEventListener("click", async () => {
        await startUpload();

        if (isUploading) {
            cancelUpload();
            return;
        }


    });
});

async function startUpload() {
    const result = document.getElementById("result");

    try {
        isUploading = true;
        uploadAbortController = new AbortController();

        setUploadButtonLoading(true);

        const request = getUploadCandlesRequest();

        result.textContent = "Loading candles...";

        const response = await fetch("/api/binance/binance-klines-upload-range", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request),
            signal: uploadAbortController.signal
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        const data = await response.json();

        result.textContent = JSON.stringify(data, null, 2);

        await loadDatabaseInfo();
    }
    catch (error) {
        if (error.name === "AbortError") {
            result.textContent = "Upload cancelled by user.";
            return;
        }

        result.textContent = `Error: ${error.message}`;
    }
    finally {
        isUploading = false;
        uploadAbortController = null;
        setUploadButtonLoading(false);
    }
}

function cancelUpload() {
    if (uploadAbortController) {
        uploadAbortController.abort();
    }
}

function setUploadButtonLoading(isLoading) {
    const spinner = document.getElementById("loadSpinner");
    const text = document.getElementById("loadButtonText");
    const button = document.getElementById("loadDataButton");

    if (spinner) {
        spinner.hidden = !isLoading;
    }

    if (text) {
        text.textContent = isLoading ? "Cancel" : "Load Data";
    }

    if (button) {
        button.classList.toggle("is-loading", isLoading);
    }
}

function getUploadCandlesRequest() {
    const symbol = document.getElementById("symbol").value.trim().toUpperCase();
    const interval = document.getElementById("interval").value.trim().toLowerCase();
    const startDate = document.getElementById("startDate").value.trim();
    const endDate = document.getElementById("endDate").value.trim();

    if (!symbol) {
        throw new Error("Symbol is required");
    }

    if (!interval) {
        throw new Error("Interval is required");
    }

    if (!startDate) {
        throw new Error("Start date is required");
    }
    const rangeData = {
        Symbol: symbol,
        Interval: interval,
        StartDate: startDate,
        EndDate: endDate || null
    };
    return rangeData;
}

async function loadDatabaseInfo() {
    const status = document.getElementById("dbInfoStatus");
    const tbody = document.getElementById("dbInfoTableBody");

    try {
        status.textContent = "Loading database information...";
        tbody.innerHTML = "";

        const response = await fetch("/api/binance/get-db-klines");

        if (!response.ok) {
            const errorText = await response.text();
            status.textContent = `HTTP ${response.status}: ${errorText}`;
            return;
        }

        const rows = await response.json();

        if (!rows.length) {
            status.textContent = "No candles found in database.";
            return;
        }
        const timeframeOrder = {
            "1m": 1,
            "5m": 2,
            "15m": 3,
            "30m": 4,
            "1h": 5,
            "4h": 6,
            "12h": 7,
            "1d": 8
        };
        const sortedRows = rows.sort((a, b) => {
            const symbolCompare = a.symbol.localeCompare(b.symbol);

            if (symbolCompare !== 0) {
                return symbolCompare;
            }

            return timeframeOrder[a.interval] - timeframeOrder[b.interval];
        });

        for (const row of sortedRows) {
            const tr = document.createElement("tr");

            tr.innerHTML = `
                <td>${row.symbol}</td>
                <td>${row.interval}</td>
                <td>${formatUnixMsUtc(row.firstOpenTimeMs)}</td>
                <td>${formatUnixMsUtc(row.lastOpenTimeMs)}</td>
                <td>${formatNumber(row.candlesCount)}</td>
            `;

            tbody.appendChild(tr);
        }

        status.textContent = `Loaded ${rows.length} datasets.`;
    }
    catch (error) {
        status.textContent = `Error: ${error.message}`;
    }
}

function formatUnixMsUtc(ms) {
    return new Date(ms)
        .toISOString()
        .slice(0, 19)
        .replace("T", " ") + " UTC";
}

function formatNumber(value) {
    return Number(value).toLocaleString();
}