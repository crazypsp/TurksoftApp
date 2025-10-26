// wwwroot/js/entities/EArchiveInvoice.js
import BaseApiService from '../Base/BaseAPIService.js';

// Controller adı (API endpoint): /api/v1/EArchiveInvoice
const svc = new BaseApiService('GibInvoice');

// Klasik CRUD
export const list = (p = null) => svc.list(p);
export const get = (id) => svc.get(id);
export const create = (d) => svc.create(d);
export const update = (id, d) => svc.update(id, d);
export const remove = (id) => svc.remove(id);

// Opsiyonel ek aksiyonlar (BaseApiService'de action/call benzeri varsa çalışır)
export const preview = (payload) => svc.action?.('preview', 'POST', payload);
export const exportPdf = (id) => svc.action?.(`${id}/pdf`, 'GET');
export const exportXml = (id) => svc.action?.(`${id}/xml`, 'GET');

export default { list, get, create, update, remove, preview, exportPdf, exportXml };
