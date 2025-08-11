import BaseAPIService from './baseAPIService.js';

const api = new BaseAPIService('https://erpapi.hizliekstre.com/api/roles');

document.addEventListener('DOMContentLoaded', () => {
  loadRoles();
});

// Tüm rolleri getirir ve tabloyu oluşturur.
async function loadRoles() {
  try {
    const roles = await api.getAll();
    renderRolesDataTable(roles);
  } catch (error) {
    console.error("Roller yüklenirken hata oluştu:", error);
  }
}

// Yeni rol oluşturur.
async function createRole(roleData) {
  try {
    const result = await api.create(roleData);
    console.log('Rol başarıyla oluşturuldu:', result);
    loadRoles(); // Tabloyu güncelle
  } catch (error) {
    console.error("Rol oluşturulurken hata oluştu:", error);
  }
}

// Rol güncelleme
async function updateRole(roleData) {
  try {
    const result = await api.update(roleData);
    console.log('Rol başarıyla güncellendi:', result);
    loadRoles(); // Tabloyu güncelle
  } catch (error) {
    console.error("Rol güncellenirken hata oluştu:", error);
  }
}

// Id ile rol getirme
async function getRoleById(id) {
  try {
    const role = await api.getById(id);
    console.log('Getirilen rol:', role);
    return role;
  } catch (error) {
    console.error("Rol getirilirken hata oluştu:", error);
  }
}

// Id ile rol silme
async function deleteRole(id) {
  try {
    const result = await api.delete(id);
    console.log('Rol başarıyla silindi:', result);
    loadRoles(); // Tabloyu güncelle
  } catch (error) {
    console.error("Rol silinirken hata oluştu:", error);
  }
}

// DataTable Render işlemi
function renderRolesDataTable(roles) {
  $('.datatables-users').DataTable({
    data: roles,
    responsive: true,
    destroy: true,
    columns: [
      { data: null, defaultContent: '' },
      { data: null, defaultContent: '' },
      { data: 'userName' },
      { data: 'roleName' },
      { data: 'plan' },
      { data: 'billing' },
      { data: 'status', render: renderStatus },
      { data: null, render: renderActions }
    ]
  });
}

// Status kolonunu formatlama
function renderStatus(data) {
  const badgeClass = data === 'Active' ? 'success' : 'danger';
  return `<span class="badge bg-label-${badgeClass}">${data}</span>`;
}

// Actions kolonunu formatlama
function renderActions(data, type, row) {
  return `
        <div class="dropdown">
            <button class="btn btn-sm dropdown-toggle" data-bs-toggle="dropdown">
                Actions
            </button>
            <ul class="dropdown-menu">
                <li><a href="#" class="dropdown-item" onclick="viewRole('${row.id}')">View</a></li>
                <li><a href="#" class="dropdown-item" onclick="editRole('${row.id}')">Edit</a></li>
                <li><a href="#" class="dropdown-item" onclick="deleteRole('${row.id}')">Delete</a></li>
            </ul>
        </div>`;
}

// Role detay görüntüleme (örnek)
window.viewRole = async function (id) {
  const role = await getRoleById(id);
  Swal.fire('Role Details', JSON.stringify(role), 'info');
};

// Rol düzenleme modal açılması (örnek kullanım)
window.editRole = async function (id) {
  const role = await getRoleById(id);
  // Modalda form doldurma işlemi (örnek kullanım)
  $('#roleNameInput').val(role.roleName);
  $('#roleIdInput').val(role.id);
  $('#addRoleModal').modal('show');
};

// Rol oluşturma/güncelleme modal işlemi
document.getElementById('saveRoleBtn').addEventListener('click', async () => {
  const roleData = {
    id: $('#roleIdInput').val(),
    roleName: $('#roleNameInput').val(),
  };

  if (roleData.id) {
    await updateRole(roleData);
  } else {
    await createRole(roleData);
  }

  $('#addRoleModal').modal('hide');
});
