/*! BaseTurkcellEfaturaApiService.js
 * ------------------------------------------------------------
 * Amaç:
 *  - API çağrıları için tek, güvenli ve tekrar kullanılabilir HTTP katmanı
 *  - 2 farklı base url şemasını destekler:
 *      1) Api:BaseUrl             => meta[name="api-base"]
 *      2) Api:GibPortalApiBaseUrl => meta[name="gib-portal-api-base"]
 *
 * Base URL çözümleme önceliği:
 *  - api:
 *      1) <meta name="api-base" content="https://.../api/v1">
 *      2) window.apiBaseUrl / window.__API_BASE
 *      3) <body data-api-base="...">
 *  - gibPortal:
 *      1) <meta name="gib-portal-api-base" content="https://.../api/">
 *      2) window.gibPortalApiBaseUrl / window.__GIB_PORTAL_API_BASE
 *      3) <body data-gib-portal-api-base="...">
 *
 * Not:
 *  - allowRelative=true ise baseUrl boş olsa bile /api/... gibi relative path’lerle çalışır.
 */

export function trimTrailingSlashes(s) {
    return String(s || "").replace(/\/+$/, "");
}
export function trimLeadingSlashes(s) {
    return String(s || "").replace(/^\/+/, "");
}

/** joinUrl("https://x.com/api", "TurkcellEFatura", "einvoice/inbox/list") */
export function joinUrl(...parts) {
    const cleaned = parts
        .filter(p => p != null && String(p).length > 0)
        .map((p, idx) => {
            const str = String(p);
            if (idx === 0) return trimTrailingSlashes(str);
            return trimLeadingSlashes(trimTrailingSlashes(str));
        });
    return cleaned.join("/");
}

function readMeta(name) {
    const meta = document.querySelector(`meta[name="${name}"]`);
    return meta && meta.content ? trimTrailingSlashes(meta.content) : "";
}

function readBodyDataset(key) {
    const b = document.body;
    if (!b || !b.dataset) return "";
    const v = b.dataset[key];
    return v ? trimTrailingSlashes(v) : "";
}

export function resolveApiBaseUrl() {
    return (
        readMeta("api-base") ||
        trimTrailingSlashes(window.apiBaseUrl || window.__API_BASE || "") ||
        readBodyDataset("apiBase") ||
        ""
    );
}

export function resolveGibPortalApiBaseUrl() {
    return (
        readMeta("gib-portal-api-base") ||
        trimTrailingSlashes(window.gibPortalApiBaseUrl || window.__GIB_PORTAL_API_BASE || "") ||
        readBodyDataset("gibPortalApiBase") ||
        ""
    );
}

function withTimeout(ms) {
    const controller = new AbortController();
    const timer = setTimeout(() => {
        try { controller.abort(); } catch (_) { /* noop */ }
    }, ms);
    return { controller, clear: () => clearTimeout(timer) };
}

function isAbsoluteUrl(path) {
    return /^https?:\/\//i.test(String(path || ""));
}

function buildQueryString(query) {
    if (!query || typeof query !== "object") return "";
    const parts = [];

    Object.keys(query).forEach((k) => {
        const v = query[k];
        if (v === undefined || v === null || v === "") return;

        // Array ise key=value1&key=value2 şeklinde yazalım
        if (Array.isArray(v)) {
            v.forEach(item => {
                if (item === undefined || item === null || item === "") return;
                parts.push(`${encodeURIComponent(k)}=${encodeURIComponent(String(item))}`);
            });
            return;
        }

        parts.push(`${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`);
    });

    return parts.length ? parts.join("&") : "";
}

async function readAsText(resp) {
    try { return await resp.text(); } catch { return ""; }
}

function tryParseJson(text) {
    if (!text) return null;
    try { return JSON.parse(text); } catch { return null; }
}

export class BaseTurkcellEfaturaApiService {
    /**
     * @param {object} options
     * @param {"api"|"gibPortal"|"relative"} [options.baseType="gibPortal"]
     * @param {string} [options.baseUrl] Elle base vermek istersen (meta’yı override eder)
     * @param {string} [options.resource] Örn: "TurkcellEFatura"
     * @param {number} [options.timeoutMs=30000]
     * @param {function(): (string|Promise<string>|null)} [options.tokenProvider] Bearer token
     * @param {boolean} [options.withCredentials=true] cookie/credentials gönderimi
     * @param {boolean} [options.allowRelative=false] base bulunamazsa relative path’lerle çalışsın mı
     */
    constructor(options = {}) {
        const baseType = options.baseType || "gibPortal";

        let base =
            options.baseUrl ||
            (baseType === "api"
                ? resolveApiBaseUrl()
                : baseType === "gibPortal"
                    ? resolveGibPortalApiBaseUrl()
                    : "");

        base = trimTrailingSlashes(base);

        if (!base && !options.allowRelative) {
            const name = baseType === "api" ? "api-base" : "gib-portal-api-base";
            throw new Error(
                `BaseUrl bulunamadı. Layout'a <meta name="${name}" ...> ekleyin veya configure ile baseUrl verin.`
            );
        }

        this.baseUrl = base; // boş olabilir (relative modda)
        this.resource = trimLeadingSlashes(trimTrailingSlashes(options.resource || ""));
        this.timeoutMs = typeof options.timeoutMs === "number" ? options.timeoutMs : 30000;
        this.tokenProvider = typeof options.tokenProvider === "function" ? options.tokenProvider : null;
        this.withCredentials = options.withCredentials === true;
        this.allowRelative = !!options.allowRelative;
    }

    async _getAuthHeader() {
        if (!this.tokenProvider) return null;
        const t = await this.tokenProvider();
        if (!t) return null;
        return `Bearer ${t}`;
    }

    _buildUrl(path, query) {
        const p = String(path || "");
        const qs = buildQueryString(query);
        const suffix = qs ? (p.includes("?") ? "&" : "?") + qs : "";

        // Tam URL verilmişse aynen kullan
        if (isAbsoluteUrl(p)) return p + suffix;

        // / ile başlıyorsa (relative absolute-path) aynen kullan
        if (p.startsWith("/")) return p + suffix;

        // base boşsa relative çalış
        if (!this.baseUrl) {
            if (!this.allowRelative) {
                throw new Error("baseUrl boş. allowRelative=true olmadan relative çağrı yapılamaz.");
            }
            // kullanıcı path'i "api/..." verirse başına "/" ekleyelim
            const rel = p.startsWith("/") ? p : "/" + p;
            return rel + suffix;
        }

        // Normal: base + resource + path
        const url = joinUrl(this.baseUrl, this.resource, p);
        return url + suffix;
    }

    /**
     * Düşük seviye istek
     * @param {string} method
     * @param {string} path
     * @param {object} opts
     * @param {"json"|"text"|"blob"} [opts.responseType="json"]
     * @param {object} [opts.query]
     * @param {any} [opts.body] (object/FormData/Blob/string)
     * @param {object} [opts.headers]
     */
    async request(method, path, opts = {}) {
        const url = this._buildUrl(path, opts.query);
        const responseType = (opts.responseType || "json").toLowerCase();

        const headers = { ...(opts.headers || {}) };
        const auth = await this._getAuthHeader();
        if (auth) headers["Authorization"] = auth;

        const body = opts.body;
        const isForm = typeof FormData !== "undefined" && body instanceof FormData;
        const isBlob = typeof Blob !== "undefined" && body instanceof Blob;
        const isString = typeof body === "string";

        // JSON body ise Content-Type set et
        const isJsonBody = body != null && !isForm && !isBlob && !isString;
        if (isJsonBody && !headers["Content-Type"]) {
            headers["Content-Type"] = "application/json";
        }

        const t = withTimeout(this.timeoutMs);

        try {
            const resp = await fetch(url, {
                method,
                headers,
                body: body == null ? undefined : (isJsonBody ? JSON.stringify(body) : body),
                credentials: this.withCredentials ? "include" : "same-origin",
                signal: t.controller.signal
            });

            if (!resp.ok) {
                // hata mesajı üret
                const text = await readAsText(resp);
                const j = tryParseJson(text);
                const msg =
                    (j && (j.message || j.error || j.detail || j.title)) ||
                    text ||
                    `HTTP ${resp.status} ${resp.statusText}`;

                const err = new Error(msg);
                err.status = resp.status;
                err.url = url;
                err.response = j || text;
                throw err;
            }

            if (responseType === "blob") return await resp.blob();
            if (responseType === "text") return await resp.text();

            // json
            const text = await readAsText(resp);
            const j = tryParseJson(text);
            return j == null ? {} : j;
        } finally {
            t.clear();
        }
    }

    // ---- Kısayollar (son kullanıcı için) ----

    /** GET (JSON) */
    get(path, query) {
        return this.request("GET", path, { query, responseType: "json" });
    }

    /** POST (JSON) */
    post(path, body, query) {
        return this.request("POST", path, { body, query, responseType: "json" });
    }

    /** PUT (JSON) */
    put(path, body, query) {
        return this.request("PUT", path, { body, query, responseType: "json" });
    }

    /** DELETE (JSON) */
    del(path, query) {
        return this.request("DELETE", path, { query, responseType: "json" });
    }

    /** POST (FormData) */
    postForm(path, formData, query) {
        return this.request("POST", path, { body: formData, query, responseType: "json" });
    }

    /** PUT (FormData) */
    putForm(path, formData, query) {
        return this.request("PUT", path, { body: formData, query, responseType: "json" });
    }

    /** İndirme (Blob) - PDF/XML/XLSX/ZIP */
    downloadBlob(path, query, method = "GET", body = null) {
        return this.request(method, path, { query, body, responseType: "blob" });
    }

    /** window.open / iframe için URL üret */
    url(path, query) {
        return this._buildUrl(path, query);
    }
}

// Debug/legacy: istersek window’a da basalım
if (typeof window !== "undefined") {
    window.BaseTurkcellEfaturaApiService = BaseTurkcellEfaturaApiService;
    window.__resolveApiBaseUrl = resolveApiBaseUrl;
    window.__resolveGibPortalApiBaseUrl = resolveGibPortalApiBaseUrl;
}
