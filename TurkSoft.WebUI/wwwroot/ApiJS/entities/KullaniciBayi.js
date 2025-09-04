// entities/KullaniciBayi.js
// ------------------------------------------------------------------
// Kullanıcı ↔ Bayi pivotu için REST sarmalayıcı (standart CRUD).
// Route: /api/v1/KullaniciBayi  (Generated.tt → [controller] = class adı)
// ------------------------------------------------------------------
import BaseApiService from '../Base/BaseAPIService.js';

const svc = new BaseApiService('KullaniciBayi');

// Standart CRUD
export const list = (p = null) => svc.list(p);
export const get = (id) => svc.get(id);
export const create = (d) => svc.create(d);   // {KullaniciId,BayiId,IsPrimary,...}
export const update = (id, d) => svc.update(id, d);
export const remove = (id) => svc.remove(id);

export default { list, get, create, update, remove };
