import { NotApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-not-body');
  const btnNew = $('#btnNewNot');
  const modalEl = $('#mdlNot');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmNot');

  // Form alanları
  const fmId = $('#frmId');
  const fmBaslik = $('#frmBaslik');
  const fmIcerik = $('#frmIcerik');
  const fmTip = $('#frmTip');
  const fmIlgiliId = $('#frmIlgiliId');
  const fmIlgiliTip = $('#frmIlgiliTip');

  // ---- Log helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
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
    const list = await NotApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Baslik ?? ''}</td>
        <td>${r.Tip ?? ''}</td>
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
    const r = await NotApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmBaslik.value = r.Baslik ?? '';
      fmIcerik.value = r.Icerik ?? '';
      fmTip.value = r.Tip ?? '';
      fmIlgiliId.value = r.IlgiliId ?? '';
      fmIlgiliTip.value = r.IlgiliTip ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await NotApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      Baslik: val(fmBaslik),
      Icerik: val(fmIcerik),
      Tip: parseInt(val(fmTip)) || 0,
      IlgiliId: val(fmIlgiliId) || null,
      IlgiliTip: val(fmIlgiliTip) || ''
    };

    if (dto.Id) {
      await NotApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await NotApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
