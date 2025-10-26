// wwwroot/js/entities/EArchiveDraft.js
import BaseApiService from '../Base/BaseAPIService.js';

// Controller endpoint → /api/v1/EArchiveDraft
const svc = new BaseApiService('GibInvoice');

// Klasik CRUD işlemleri
export const list = (p = null) => svc.list(p);
export const get = (id) => svc.get(id);
export const create = (d) => svc.create(d);
export const update = (id, d) => svc.update(id, d);
export const remove = (id) => svc.remove(id);

// Toplu işlemler (opsiyonel: backend varsa)
export const sendAll = (ids) => svc.action?.('send-all', 'POST', { ids });
export const deleteAll = (ids) => svc.action?.('delete-all', 'POST', { ids });
export const uploadExcel = (formData) => svc.upload?.('excel-import', formData);

export default { list, get, create, update, remove, sendAll, deleteAll, uploadExcel };
