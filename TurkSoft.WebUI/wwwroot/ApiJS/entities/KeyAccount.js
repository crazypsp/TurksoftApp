
// Kullanıcı endpoint’i için REST sarmalayıcı.
import BaseApiService from '../Base/BaseAPIService.js'; // DİKKAT: isim birebir

const RESOURCE = 'keyaccount';
const svc = new BaseApiService(RESOURCE);

export const list = (params = null) => svc.list(params);
export const get = (id) => svc.get(id);
export const create = (data) => svc.create(data);
export const update = (id, data) => svc.update(id, data);
export const remove = (id) => svc.remove(id);

export default { list, get, create, update, remove };
