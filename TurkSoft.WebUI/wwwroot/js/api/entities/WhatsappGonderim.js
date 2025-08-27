
// wwwroot/js/api/entities/WhatsappGonderim.js
// ======================================================================
// Bu dosya otomatik üretildi. İlgili entity için REST API sarmalayıcısı.
// - API kök adresini _Layout.cshtml meta etiketi üzerinden alır (BaseApiService).
// - Her satır, kurumsal bakım kolaylığı için açıklamalıdır.
// ======================================================================

import BaseApiService from '../core/BaseApiService.js'; // Ortak REST tabanı

// (1) Controller segmenti: backend'deki route segmenti (örn: 'firma', 'kullanici').
const RESOURCE = 'whatsappgonderim';

// (2) (Opsiyonel) Kimlik doğrulama gerekiyorsa token sağlayıcıyı burada uygula.
const getToken = () => null; // Ör: return localStorage.getItem('access_token');

// (3) Bu entity için tekil servis örneği oluşturuyoruz.
const svc = new BaseApiService(RESOURCE, getToken);

// (4) CRUD işlevleri
async function list(params = null)  { return svc.list(params);  } // GET /whatsappgonderim
async function get(id)              { return svc.get(id);        } // GET /whatsappgonderim/{id}
async function create(data)         { return svc.create(data);   } // POST /whatsappgonderim
async function update(id, data)     { return svc.update(id, data);} // PUT  /whatsappgonderim/{id}
async function remove(id)           { return svc.remove(id);     } // DELETE /whatsappgonderim/{id}

// (5) Export
export const WhatsappGonderimApi = { list, get, create, update, remove };
export default WhatsappGonderimApi;
