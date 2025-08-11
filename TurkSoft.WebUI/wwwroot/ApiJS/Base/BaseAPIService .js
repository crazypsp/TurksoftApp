export default class BaseAPIService {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async request(endpoint, method, data = null) {
    const options = {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: data ? JSON.stringify(data) : null
    };

    const res = await fetch(`${this.baseUrl}/${endpoint}`, options);
    if (!res.ok) throw new Error(`API Error: ${res.status}`);
    return await res.json();
  }

  getAll(endpoint = 'GetAll') {
    return this.request(endpoint, 'GET');
  }

  getById(id, endpoint = 'GetById') {
    return this.request(`${endpoint}/${id}`, 'GET');
  }

  create(data, endpoint = 'Create') {
    return this.request(endpoint, 'POST', data);
  }

  update(data, endpoint = 'Update') {
    return this.request(endpoint, 'PUT', data);
  }

  delete(id, endpoint = 'Delete') {
    return this.request(`${endpoint}/${id}`, 'DELETE');
  }
}
