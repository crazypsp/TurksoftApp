import BaseApiService from '../Base/BaseAPIService.js';

const svc = new BaseApiService('BankStatements', null, 90000);

// POST /api/BankStatements/statement
export const statement = (dto) => svc.postPath('statement', dto);

export default { statement };
