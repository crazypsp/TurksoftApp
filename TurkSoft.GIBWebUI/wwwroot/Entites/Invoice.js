// wwwroot/js/entities/Invoice.js
import BaseApiService from '../Base/BaseAPIService.js';

// Controller ismiyle birebir aynı olmalı
const svc = new BaseApiService('GibInvoice');

export const list = (p = null) => svc.list(p);
export const get = (id) => svc.get(id);
export const create = (d) => svc.create(d);
export const update = (id, d) => svc.update(id, d);
export const remove = (id) => svc.remove(id);

export default { list, get, create, update, remove };
