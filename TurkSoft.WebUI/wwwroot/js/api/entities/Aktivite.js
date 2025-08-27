
// wwwroot/js/api/entities/Aktivite.js
// ======================================================================
// Bu dosya otomatik üretildi. İlgili entity için REST API sarmalayıcısı.
// - API kök adresini _Layout.cshtml meta etiketi üzerinden alır (BaseApiService).
// - Her satır, kurumsal bakım kolaylığı için açıklamalıdır.
// ======================================================================

import BaseApiService from '../core/BaseApiService.js'; // Ortak REST tabanı

// (1) Controller segmenti: backend'deki route segmenti (örn: 'firma', 'kullanici').
const RESOURCE = 'aktivite';

// (2) (Opsiyonel) Kimlik doğrulama gerekiyorsa token sağlayıcıyı burada uygula.
const getToken = () => null; // Ör: return localStorage.getItem('access_token');

// (3) Bu entity için tekil servis örneği oluşturuyoruz.
const svc = new BaseApiService(RESOURCE, getToken);

// (4) CRUD işlevleri
async function list(params = null)  { return svc.list(params);  } // GET /aktivite
async function get(id)              { return svc.get(id);        } // GET /aktivite/{id}
async function create(data)         { return svc.create(data);   } // POST /aktivite
async function update(id, data)     { return svc.update(id, data);} // PUT  /aktivite/{id}
async function remove(id)           { return svc.remove(id);     } // DELETE /aktivite/{id}

// (5) Export
export const AktiviteApi = { list, get, create, update, remove };
export default AktiviteApi;
