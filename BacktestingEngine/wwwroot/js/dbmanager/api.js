
// @ts-check

/**
 * @param {RequestInit["method"]} method
 * @param {string} url
 * @param {Object} [body]
 * @returns {Promise<Response>}
 * 
 */

export async function request(method, url, body) {
    const options = { method };

    if (body !== undefined) {
        options.headers = { "Content-Type": "application/json" };
        options.body = JSON.stringify(body);
    }

    const response = await fetch(url, options);

    if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${await response.text()}`);
    }
    return response;
}
