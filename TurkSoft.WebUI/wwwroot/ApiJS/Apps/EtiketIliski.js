import { EtiketIliskiApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-etiketiliski-body');
  const btnNew = $('#btnNewEtiketIliski');
  const modalEl = $('#mdlEtiketIliski');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmEtiketIliski');

  // Form alanları
  const fmId = $('#frmId');
  const fmEtiketId = $('#frmEtiketId');
  const fmIlgiliId = $('#frmIlgiliId');
  const fmIlgiliTip = $('#frmIlgiliTip');

  // ---- Log ekleme helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession(); // { Id, ... }
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log yazılamadı:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await EtiketIliskiApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.EtiketId ?? ''}</td>
        <td>${r.IlgiliId ?? ''}</td>
        <td>${r.IlgiliTip ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await EtiketIliskiApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);
      fmId.value = r.Id;
      fmEtiketId.value = r.EtiketId ?? '';
      fmIlgiliId.value = r.IlgiliId ?? '';
      fmIlgiliTip.value = r.IlgiliTip ?? '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await EtiketIliskiApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt butonu ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Kayıt ekleme / güncelleme ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      EtiketId: val(fmEtiketId),
      IlgiliId: val(fmIlgiliId),
      IlgiliTip: val(fmIlgiliTip)
    };

    if (dto.Id) {
      await EtiketIliskiApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await EtiketIliskiApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
