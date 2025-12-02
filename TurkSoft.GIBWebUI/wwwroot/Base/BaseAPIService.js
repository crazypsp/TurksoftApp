// wwwroot/js/Base/BaseAPIService.js

// API base adresini <meta name="api-base" /> ya da window.__API_BASE vs.'den okur
function getApiBaseFromMeta() {
    const meta = document.querySelector('meta[name="api-base"]');
    if (meta && meta.content) return meta.content.replace(/\/+$/, '');

    // Fallback: window değişkeni
    if (typeof window !== 'undefined' && window.__API_BASE) {
        return String(window.__API_BASE).replace(/\/+$/, '');
    }

    // Opsiyonel: body data-attrib
    const body = document.getElementById('MainBody');
    if (body && body.dataset && body.dataset.apiBase)
        return String(body.dataset.apiBase).replace(/\/+$/, '');

    return '';
}

const defaultGetToken = () => null;

// Log’larda parolayı maskelemek için güvenli stringify
function safeStringify(obj) {
    try {
        const clone = JSON.parse(JSON.stringify(obj ?? {}));
        ['password', 'pwd', 'sifre', 'parola'].forEach(k => {
            Object.keys(clone).forEach(kk => {
                if (kk.toLowerCase().includes(k)) clone[kk] = '********';
            });
        });
        return JSON.stringify(clone);
    } catch {
        return '[unserializable]';
    }
}

// ASP.NET Core validation / ProblemDetails error’larını tek mesaja çevirir
function extractAspNetError(payload) {
    if (!payload || typeof payload !== 'object') return null;

    // ModelState errors
    if (payload.errors && typeof payload.errors === 'object') {
        const parts = [];
        for (const [field, arr] of Object.entries(payload.errors)) {
            const msgs = Array.isArray(arr) ? arr.join(' | ') : String(arr);
            parts.push(`${field}: ${msgs}`);
        }
        if (parts.length) return parts.join(' || ');
    }

    if (payload.detail) return String(payload.detail);
    if (payload.title) return String(payload.title);
    if (payload.message) return String(payload.message);

    return null;
}

export default class BaseApiService {
    /**
     * @param {string} resource  Controller adı (örn: 'GibUser', 'GibGibFirm')
     * @param {() => string|null} getToken  (opsiyonel) Auth token sağlayan fonksiyon
     * @param {number} timeoutMs  İstek timeout (ms)
     */
    constructor(resource, getToken = defaultGetToken, timeoutMs = 30000) {
        if (!resource) throw new Error('BaseApiService: resource zorunlu.');

        this.resource = String(resource).replace(/^\/+/, '');
        this.getToken = getToken;
        this.timeoutMs = timeoutMs;

        this.baseUrl = getApiBaseFromMeta();
        if (!this.baseUrl) {
            throw new Error('API kök adresi bulunamadı (meta api-base).');
        }
    }

    // /api/v1/{resource}/[path]
    _url(path = '') {
        const p = path ? `/${String(path).replace(/^\/+/, '')}` : '';
        return `${this.baseUrl}/${this.resource}${p}`;
    }

    // Query string helper
    _qs(params) {
        if (!params) return '';
        const usp = new URLSearchParams();

        Object.entries(params).forEach(([k, v]) => {
            if (v == null) return;
            if (Array.isArray(v)) {
                v.forEach(x => usp.append(k, x));
            } else {
                usp.append(k, String(v));
            }
        });

        const s = usp.toString();
        return s ? `?${s}` : '';
    }

    async _request(input, { method = 'GET', body = null, headers = {} } = {}) {
        const ctrl = new AbortController();
        const t = setTimeout(() => ctrl.abort(), this.timeoutMs);

        const h = { Accept: 'application/json', ...headers };
        if (body != null) h['Content-Type'] = 'application/json';

        const token = this.getToken ? this.getToken() : null;
        if (token) h['Authorization'] = `Bearer ${token}`;

        console.debug('[API] →', method, input, body ? safeStringify(body) : '');

        try {
            const res = await fetch(input, {
                method,
                headers: h,
                body: body != null ? JSON.stringify(body) : undefined,
                signal: ctrl.signal,
                credentials: 'omit'
            });

            if (res.status === 204) return null;

            const ct = res.headers.get('content-type') || '';
            const isJson = ct.includes('application/json');
            const data = isJson
                ? await res.json().catch(() => null)
                : await res.text().catch(() => null);

            if (!res.ok) {
                const errMsg = extractAspNetError(data);
                const err = new Error(errMsg ? errMsg : `API Error ${res.status}`);
                err.status = res.status;
                err.payload = data;
                throw err;
            }

            return data;
        } finally {
            clearTimeout(t);
        }
    }

    // ==== REST shortcut metotları ====

    // GET /{resource}?...
    list(params = null) {
        return this._request(this._url() + this._qs(params));
    }

    // GET /{resource}/{id}
    get(id) {
        if (!id && id !== 0) throw new Error('get(id): id zorunlu');
        return this._request(this._url(id));
    }

    // POST /{resource}
    create(data) {
        return this._request(this._url(), {
            method: 'POST',
            body: data
        });
    }

    // PUT /{resource}/{id}
    update(id, data) {
        if (!id && id !== 0) throw new Error('update(id, data): id zorunlu');
        return this._request(this._url(id), {
            method: 'PUT',
            body: data
        });
    }

    // DELETE /{resource}/{id}
    remove(id) {
        if (!id && id !== 0) throw new Error('remove(id): id zorunlu');
        return this._request(this._url(id), {
            method: 'DELETE'
        });
    }
}
