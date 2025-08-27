// wwwroot/js/api/core/BaseApiService.js
// ======================================================================
// Amaç: Tüm entity servisleri için ortak, sağlam REST katmanı.
// - API kök adresini _Layout.cshtml'deki <meta name="api-base"> içinden alır.
// - JSON ve FormData (multipart) isteklerini destekler.
// - Timeout (AbortController), token (opsiyonel), istisna yönetimi içerir.
// - Aynı origin / farklı domain için uygun credentials modunu otomatik seçer.
// - İleriye dönük genişlemeler için esnek yardımcı metotlar sağlar.
// ======================================================================

/**
 * _Layout.cshtml içinde tanımlanan meta'dan API kök adresini okur.
 * <meta name="api-base" content="https://localhost:7109/api/v1" />
 */
function getApiBaseFromMeta() {
  const meta = document.querySelector('meta[name="api-base"]');
  const val = meta && meta.content ? meta.content.trim() : '';
  if (!val) {
    // Geliştirmede net uyarı vermek faydalıdır (prod’da da sorun değil).
    console.warn('[BaseApiService] <meta name="api-base"> bulunamadı veya boş.');
  }
  // Sondaki fazla / işaretlerini kırp (örn. .../api/v1////)
  return val.replace(/\/+$/, '');
}

/**
 * Verilen URL, mevcut sayfanın origin’i ile aynı mı?
 * (CORS ve credentials modu için kullanılır.)
 */
function isSameOrigin(url) {
  try {
    const u = new URL(url, window.location.origin);
    return u.origin === window.location.origin;
  } catch {
    return false;
  }
}

/** Varsayılan token sağlayıcı (kimlik doğrulama yoksa null döner). */
const defaultGetToken = () => null;

/** Hata nesnesini tek tip oluşturmak için yardımcı. */
function buildHttpError(res, payload) {
  const err = new Error(`API Error ${res.status}`);
  err.status = res.status;
  err.payload = payload; // ProblemDetails / text / null olabilir
  err.url = res.url;
  return err;
}

export default class BaseApiService {
  /**
   * @param {string} resource   Controller segmenti (örn: "firma", "kullanici")
   * @param {() => (string|null)} getToken  Opsiyonel bearer token sağlayıcı
   * @param {number} timeoutMs  İstek zaman aşımı (ms). Varsayılan: 30000
   * @param {string|null} baseUrlOverride  (Opsiyonel) meta yerine doğrudan base URL
   */
  constructor(resource, getToken = defaultGetToken, timeoutMs = 30000, baseUrlOverride = null) {
    if (!resource) throw new Error('BaseApiService: "resource" zorunludur.');
    this.resource = String(resource).replace(/^\/+/, ''); // baştaki / işaretlerini temizle
    this.getToken = getToken;                             // token sağlayıcı callback
    this.timeoutMs = timeoutMs;                           // default timeout
    this.baseUrl = (baseUrlOverride || getApiBaseFromMeta() || '').replace(/\/+$/, '');
    if (!this.baseUrl) {
      throw new Error('BaseApiService: API kök adresi bulunamadı. _Layout.cshtml <meta name="api-base"> eksik olabilir.');
    }
  }

  // ------------------------------------------------------------------
  // Konfigürasyon set ediciler (runtime’da değiştirmek istersen faydalı)
  // ------------------------------------------------------------------

  /** Token sağlayıcıyı sonradan değiştirmek için. */
  setTokenProvider(fn) {
    this.getToken = typeof fn === 'function' ? fn : defaultGetToken;
  }

  /** Timeout süresini güncellemek için. */
  setTimeout(ms) {
    if (Number.isFinite(ms) && ms > 0) this.timeoutMs = ms;
  }

  /** Base URL’yi güncellemek için. */
  setBaseUrl(url) {
    if (typeof url === 'string' && url.trim()) {
      this.baseUrl = url.trim().replace(/\/+$/, '');
    }
  }

  // ------------------------------------------------------------------
  // URL & Query yardımcıları
  // ------------------------------------------------------------------

  /** Kaynak URL’yi üret: {base}/{resource}/{path?} */
  _url(path = '') {
    const p = path ? `/${String(path).replace(/^\/+/, '')}` : '';
    return `${this.baseUrl}/${this.resource}${p}`;
  }

  /** QueryString üret: { a:1, b:'x', tags:['a','b'] } → ?a=1&b=x&tags=a&tags=b */
  _qs(params) {
    if (!params) return '';
    const usp = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v == null) return; // null/undefined atla
      if (Array.isArray(v)) v.forEach(x => usp.append(k, x));
      else usp.append(k, String(v));
    });
    const s = usp.toString();
    return s ? `?${s}` : '';
  }

  /** Body’ye göre Content-Type belirleme (FormData ise tarayıcı ayarlasın). */
  _composeHeaders(userHeaders = {}, body = null, token = null) {
    const headers = { Accept: 'application/json', ...userHeaders };

    // FormData gönderirken Content-Type’ı elle set ETME; boundary’yi tarayıcı koymalı.
    const isForm = (typeof FormData !== 'undefined') && (body instanceof FormData);
    if (!isForm && body != null && !headers['Content-Type']) {
      headers['Content-Type'] = 'application/json';
    }

    if (token && !headers['Authorization']) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
  }

  // ------------------------------------------------------------------
  // Çekirdek istek metodu
  // ------------------------------------------------------------------

  /**
   * @param {string} input               Tam URL
   * @param {object} options
   * @param {'GET'|'POST'|'PUT'|'DELETE'|'PATCH'} [options.method='GET']
   * @param {any}    [options.body=null] JSON objesi veya FormData
   * @param {object} [options.headers={}] İlave header’lar
   * @param {number} [options.timeoutMs] Bu isteğe özel timeout (ms)
   * @param {'json'|'text'|'blob'} [options.responseType='json'] Beklenen yanıt türü
   */
  async _request(input, {
    method = 'GET',
    body = null,
    headers = {},
    timeoutMs,
    responseType = 'json'
  } = {}) {
    // AbortController ile zaman aşımı
    const ctrl = new AbortController();
    const to = Number.isFinite(timeoutMs) ? timeoutMs : this.timeoutMs;
    const timer = setTimeout(() => ctrl.abort(), to);

    const token = this.getToken ? this.getToken() : null;
    const composedHeaders = this._composeHeaders(headers, body, token);

    // Aynı origin ise 'same-origin', aksi halde 'include' (CORS için)
    const creds = isSameOrigin(this.baseUrl) ? 'same-origin' : 'include';

    // FormData değilse JSON string'e çevir
    const isForm = (typeof FormData !== 'undefined') && (body instanceof FormData);
    const finalBody = isForm ? body : (body != null ? JSON.stringify(body) : undefined);

    try {
      const res = await fetch(input, {
        method,
        headers: composedHeaders,
        body: finalBody,
        signal: ctrl.signal,
        credentials: creds
      });

      // 204 No Content → null dön
      if (res.status === 204) return null;

      // İçerik türünü kontrol et
      const contentType = res.headers.get('content-type') || '';
      const looksJson = contentType.includes('application/json');

      // Başarısız (4xx/5xx) ise uygun payload ile hata fırlat
      if (!res.ok) {
        let payload = null;
        try {
          payload = looksJson ? await res.json() : await res.text();
        } catch {
          payload = null;
        }
        throw buildHttpError(res, payload);
      }

      // Başarılı — responseType tercihini uygula
      if (responseType === 'blob') return await res.blob();
      if (responseType === 'text') return await res.text();
      // Varsayılan JSON
      return looksJson ? await res.json() : await res.text();

    } catch (e) {
      if (e.name === 'AbortError') {
        const err = new Error('İstek zaman aşımına uğradı.');
        err.status = 408;
        err.url = input;
        throw err;
      }
      throw e;
    } finally {
      clearTimeout(timer);
    }
  }

  // ------------------------------------------------------------------
  // Standart CRUD (JSON)
  // ------------------------------------------------------------------

  /** Listeleme: GET /{resource}?{query} */
  async list(params = null) {
    return this._request(this._url() + this._qs(params), { method: 'GET' });
  }

  /** Tek kayıt: GET /{resource}/{id} */
  async get(id) {
    if (!id) throw new Error('get(id): "id" zorunludur.');
    return this._request(this._url(id), { method: 'GET' });
  }

  /** Oluştur: POST /{resource} (JSON) */
  async create(data) {
    if (!data) throw new Error('create(data): "data" zorunludur.');
    return this._request(this._url(), { method: 'POST', body: data });
  }

  /** Güncelle: PUT /{resource}/{id} (JSON) */
  async update(id, data) {
    if (!id || !data) throw new Error('update(id, data): "id" ve "data" zorunludur.');
    return this._request(this._url(id), { method: 'PUT', body: data });
  }

  /** Sil: DELETE /{resource}/{id} */
  async remove(id) {
    if (!id) throw new Error('remove(id): "id" zorunludur.');
    return this._request(this._url(id), { method: 'DELETE' });
  }

  // ------------------------------------------------------------------
  // FormData (multipart) yardımcıları — dosya yükleme vb. senaryolar için
  // ------------------------------------------------------------------

  /**
   * FormData ile oluşturma: POST /{resource}
   * @param {FormData} formData
   */
  async createForm(formData) {
    if (!(formData instanceof FormData)) {
      throw new Error('createForm(formData): FormData bekleniyor.');
    }
    return this._request(this._url(), { method: 'POST', body: formData });
  }

  /**
   * FormData ile güncelleme: PUT /{resource}/{id}
   * @param {string} id
   * @param {FormData} formData
   */
  async updateForm(id, formData) {
    if (!id || !(formData instanceof FormData)) {
      throw new Error('updateForm(id, formData): "id" ve FormData zorunludur.');
    }
    return this._request(this._url(id), { method: 'PUT', body: formData });
  }

  // ------------------------------------------------------------------
  // İndirme (blob) — dosya indirme senaryosu (örn. fatura PDF)
  // ------------------------------------------------------------------

  /**
   * Dosya indirme: GET /{resource}/{path}?{query} → Blob
   * @param {string} path   id veya özel alt yol
   * @param {object|null} params  query parametreleri
   */
  async download(path, params = null) {
    const url = this._url(path) + this._qs(params);
    return this._request(url, { method: 'GET', responseType: 'blob' });
  }
}
