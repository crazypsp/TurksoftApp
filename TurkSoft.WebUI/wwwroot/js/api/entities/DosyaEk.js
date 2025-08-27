
// wwwroot/js/api/entities/DosyaEk.js
// ======================================================================
// Bu dosya otomatik üretildi. İlgili entity için REST API sarmalayıcısı.
// - API kök adresini _Layout.cshtml meta etiketi üzerinden alır (BaseApiService).
// - Her satır, kurumsal bakım kolaylığı için açıklamalıdır.
// ======================================================================

import BaseApiService from '../core/BaseApiService.js'; // Ortak REST tabanı

// (1) Controller segmenti: backend'deki route segmenti (örn: 'firma', 'kullanici').
const RESOURCE = 'dosyaek';

// (2) (Opsiyonel) Kimlik doğrulama gerekiyorsa token sağlayıcıyı burada uygula.
const getToken = () => null; // Ör: return localStorage.getItem('access_token');

// (3) Bu entity için tekil servis örneği oluşturuyoruz.
const svc = new BaseApiService(RESOURCE, getToken);

// (4) CRUD işlevleri
async function list(params = null)  { return svc.list(params);  } // GET /dosyaek
async function get(id)              { return svc.get(id);        } // GET /dosyaek/{id}
async function create(data)         { return svc.create(data);   } // POST /dosyaek
async function update(id, data)     { return svc.update(id, data);} // PUT  /dosyaek/{id}
async function remove(id)           { return svc.remove(id);     } // DELETE /dosyaek/{id}

// (5) Export
export const DosyaEkApi = { list, get, create, update, remove };
export default DosyaEkApi;
