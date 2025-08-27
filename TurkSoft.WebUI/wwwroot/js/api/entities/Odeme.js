
// wwwroot/js/api/entities/Odeme.js
// ======================================================================
// Bu dosya otomatik üretildi. İlgili entity için REST API sarmalayıcısı.
// - API kök adresini _Layout.cshtml meta etiketi üzerinden alır (BaseApiService).
// - Her satır, kurumsal bakım kolaylığı için açıklamalıdır.
// ======================================================================

import BaseApiService from '../core/BaseApiService.js'; // Ortak REST tabanı

// (1) Controller segmenti: backend'deki route segmenti (örn: 'firma', 'kullanici').
const RESOURCE = 'odeme';

// (2) (Opsiyonel) Kimlik doğrulama gerekiyorsa token sağlayıcıyı burada uygula.
const getToken = () => null; // Ör: return localStorage.getItem('access_token');

// (3) Bu entity için tekil servis örneği oluşturuyoruz.
const svc = new BaseApiService(RESOURCE, getToken);

// (4) CRUD işlevleri
async function list(params = null)  { return svc.list(params);  } // GET /odeme
async function get(id)              { return svc.get(id);        } // GET /odeme/{id}
async function create(data)         { return svc.create(data);   } // POST /odeme
async function update(id, data)     { return svc.update(id, data);} // PUT  /odeme/{id}
async function remove(id)           { return svc.remove(id);     } // DELETE /odeme/{id}

// (5) Export
export const OdemeApi = { list, get, create, update, remove };
export default OdemeApi;
