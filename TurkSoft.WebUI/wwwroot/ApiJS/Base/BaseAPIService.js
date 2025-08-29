// Ortak REST tabanı. API kök adresini <meta name="api-base">’den okur.
function getApiBaseFromMeta() {
  const meta = document.querySelector('meta[name="api-base"]');
  return meta && meta.content ? meta.content.replace(/\/+$/, '') : '';
}
const defaultGetToken = () => null;

export default class BaseApiService {
  constructor(resource, getToken = defaultGetToken, timeoutMs = 30000) {
    if (!resource) throw new Error('BaseApiService: resource zorunlu.');
    this.resource = String(resource).replace(/^\/+/, '');
    this.getToken = getToken;
    this.timeoutMs = timeoutMs;
    this.baseUrl = getApiBaseFromMeta();
    if (!this.baseUrl) throw new Error('API kök adresi bulunamadı (meta api-base).');
  }
  _url(path = '') { const p = path ? `/${String(path).replace(/^\/+/, '')}` : ''; return `${this.baseUrl}/${this.resource}${p}`; }
  _qs(params) { if (!params) return ''; const usp = new URLSearchParams(); Object.entries(params).forEach(([k, v]) => { if (v == null) return; Array.isArray(v) ? v.forEach(x => usp.append(k, x)) : usp.append(k, String(v)); }); const s = usp.toString(); return s ? `?${s}` : ''; }
  async _request(input, { method = 'GET', body = null, headers = {} } = {}) {
    const ctrl = new AbortController(); const t = setTimeout(() => ctrl.abort(), this.timeoutMs);
    const h = { Accept: 'application/json', ...headers }; if (body != null) h['Content-Type'] = 'application/json';
    const token = this.getToken ? this.getToken() : null; if (token) h['Authorization'] = `Bearer ${token}`;
    try {
      const res = await fetch(input, { method, headers: h, body: body != null ? JSON.stringify(body) : undefined, signal: ctrl.signal, credentials: 'omit' });
      if (res.status === 204) return null;
      const isJson = (res.headers.get('content-type') || '').includes('application/json');
      if (!res.ok) { const payload = isJson ? await res.json().catch(() => null) : await res.text().catch(() => null); const err = new Error(`API Error ${res.status}`); err.status = res.status; err.payload = payload; throw err; }
      return isJson ? await res.json() : await res.text();
    } catch (e) { if (e.name === 'AbortError') { const err = new Error('İstek zaman aşımı'); err.status = 408; throw err; } throw e; }
    finally { clearTimeout(t); }
  }
  list(params = null) { return this._request(this._url() + this._qs(params), { method: 'GET' }); }
  get(id) { if (!id) throw new Error('get(id): id zorunlu'); return this._request(this._url(id), { method: 'GET' }); }
  create(data) { if (!data) throw new Error('create(data): data zorunlu'); return this._request(this._url(), { method: 'POST', body: data }); }
  update(id, data) { if (!id || !data) throw new Error('update: id ve data zorunlu'); return this._request(this._url(id), { method: 'PUT', body: data }); }
  remove(id) { if (!id) throw new Error('remove(id): id zorunlu'); return this._request(this._url(id), { method: 'DELETE' }); }
}
