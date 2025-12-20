function getApiBaseFromMeta() {
    const meta = document.querySelector('meta[name="api-base"]');
    return meta && meta.content ? meta.content.replace(/\/+$/, '') : '';
}

function getApiKeyFromMetaOrStorage() {
    // İstersen kullanıcı bazlı: localStorage.setItem('TS_API_KEY','...')
    const ls = localStorage.getItem('TS_API_KEY');
    if (ls) return ls;

    const meta = document.querySelector('meta[name="api-key"]');
    return meta && meta.content ? meta.content : '';
}

function getApiKeyHeaderNameFromMeta() {
    const meta = document.querySelector('meta[name="api-key-header"]');
    return meta && meta.content ? meta.content : 'X-API-KEY';
}

const defaultGetToken = () => null;

function safeStringify(obj) {
    try {
        const clone = JSON.parse(JSON.stringify(obj ?? {}));
        ['password', 'pwd', 'sifre', 'parola'].forEach(k => {
            Object.keys(clone).forEach(kk => {
                if (kk.toLowerCase().includes(k)) clone[kk] = '********';
            });
        });
        return JSON.stringify(clone);
    } catch { return '[unserializable]'; }
}

function extractAspNetError(payload) {
    if (!payload || typeof payload !== 'object') return null;

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
    constructor(resource, getToken = defaultGetToken, timeoutMs = 30000) {
        if (!resource) throw new Error('BaseApiService: resource zorunlu.');
        this.resource = String(resource).replace(/^\/+/, '');
        this.getToken = getToken;
        this.timeoutMs = timeoutMs;
        this.baseUrl = getApiBaseFromMeta();
        if (!this.baseUrl) throw new Error('API kök adresi bulunamadı (meta api-base).');
    }

    _url(path = '') {
        const p = path ? `/${String(path).replace(/^\/+/, '')}` : '';
        return `${this.baseUrl}/${this.resource}${p}`;
    }

    _qs(params) {
        if (!params) return '';
        const usp = new URLSearchParams();
        Object.entries(params).forEach(([k, v]) => {
            if (v == null) return;
            Array.isArray(v) ? v.forEach(x => usp.append(k, x)) : usp.append(k, String(v));
        });
        const s = usp.toString();
        return s ? `?${s}` : '';
    }

    async _request(input, { method = 'GET', body = null, headers = {} } = {}) {
        const ctrl = new AbortController();
        const t = setTimeout(() => ctrl.abort(), this.timeoutMs);

        const h = { Accept: 'application/json', ...headers };
        if (body != null) h['Content-Type'] = 'application/json';

        // Bearer token (senin standart)
        const token = this.getToken ? this.getToken() : null;
        if (token) h['Authorization'] = `Bearer ${token}`;

        // ✅ API KEY (BankServiceAPI için)
        const apiKey = getApiKeyFromMetaOrStorage();
        if (apiKey) {
            const headerName = getApiKeyHeaderNameFromMeta() || 'X-API-KEY';
            h[headerName] = apiKey;
        }

        console.debug('[API] →', method, input, body ? safeStringify(body) : '');

        try {
            const res = await fetch(input, {
                method,
                headers: h,
                body: body != null ? JSON.stringify(body) : undefined,
                signal: ctrl.signal,
                credentials: 'omit'
            });

            if (res.status === 204) { console.debug('[API] ← 204 NoContent'); return null; }

            const ct = res.headers.get('content-type') || '';
            const isJson = ct.includes('application/json');
            const data = isJson ? await res.json().catch(() => null) : await res.text().catch(() => null);

            if (!res.ok) {
                const errMsg = extractAspNetError(data);
                const err = new Error(errMsg ? errMsg : `API Error ${res.status}`);
                err.status = res.status;
                err.payload = data;
                console.error('[API-ERR] ←', res.status, err.message, data);
                throw err;
            }

            console.debug('[API] ←', res.status, isJson ? data : '[text]');
            return data;
        } catch (e) {
            if (e.name === 'AbortError') {
                const err = new Error('İstek zaman aşımı'); err.status = 408; throw err;
            }
            throw e;
        } finally {
            clearTimeout(t);
        }
    }

    postPath(path, data) { return this._request(this._url(path), { method: 'POST', body: data }); }
    deletePath(path) { return this._request(this._url(path), { method: 'DELETE' }); }

    list(params = null) { return this._request(this._url() + this._qs(params), { method: 'GET' }); }
    get(id) { if (!id) throw new Error('get(id): id zorunlu'); return this._request(this._url(id), { method: 'GET' }); }
    create(data) { if (!data) throw new Error('create(data): data zorunlu'); return this._request(this._url(), { method: 'POST', body: data }); }
    update(id, data) { if (!id || !data) throw new Error('update: id ve data zorunlu'); return this._request(this._url(id), { method: 'PUT', body: data }); }
    remove(id) { if (!id) throw new Error('remove(id): id zorunlu'); return this._request(this._url(id), { method: 'DELETE' }); }
}
