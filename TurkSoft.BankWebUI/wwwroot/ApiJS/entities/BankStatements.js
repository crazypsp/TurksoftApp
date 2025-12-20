import BaseApiService from '../Base/BaseAPIService.js';

// BankServiceAPI route: /api/BankStatements/statement
const svc = new BaseApiService('BankStatements');

export const statement = (dto) => svc.postPath('statement', dto);

export default { statement };
