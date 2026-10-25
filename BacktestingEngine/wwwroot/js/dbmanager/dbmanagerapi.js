// @ts-check

import { request } from "./api.js";

/**
 * @param {string} symbol
 * @param {string} interval
 * @returns {Promise<void>}
 */
export async function deleteDataset(symbol, interval) {
    const response = await request("DELETE", "/api/binance/delete-db-klines", {
            Symbol: symbol,
            Interval: interval
        });
}


export async function loadDatabaseInfo() {
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
            <td></td>
            `;

            tr.lastElementChild.appendChild(createDeleteButton(row.symbol, row.interval));
            tbody.appendChild(tr);
        }

        status.textContent = `Loaded ${rows.length} datasets.`;
    }
    catch (error) {
        status.textContent = `Error: ${error.message}`;
    }
}













