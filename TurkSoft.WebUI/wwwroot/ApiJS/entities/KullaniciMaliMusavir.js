// entities/KullaniciMaliMusavir.js
// ------------------------------------------------------------------
// Kullanıcı ↔ MaliMüşavir pivotu için REST sarmalayıcı (standart CRUD).
// Route: /api/v1/KullaniciMaliMusavir
// ------------------------------------------------------------------
import BaseApiService from '../Base/BaseAPIService.js';

const svc = new BaseApiService('KullaniciMaliMusavir');

// Standart CRUD
export const list = (p = null) => svc.list(p);
export const get = (id) => svc.get(id);
export const create = (d) => svc.create(d);   // {KullaniciId,MaliMusavirId,IsPrimary,...}
export const update = (id, d) => svc.update(id, d);
export const remove = (id) => svc.remove(id);

export default { list, get, create, update, remove };
