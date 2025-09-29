import { OutboxMesajApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-outboxmesaj-body');
  const btnNew = $('#btnNewOutboxMesaj');
  const modalEl = $('#mdlOutboxMesaj');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmOutboxMesaj');

  // Form alanları
  const fmId = $('#frmId');
  const fmTip = $('#frmTip');
  const fmIcerikJson = $('#frmIcerikJson');
  const fmIslenmis = $('#frmIslenmis');
  const fmIslenmeTarihi = $('#frmIslenmeTarihi');

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
    const list = await OutboxMesajApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Tip ?? ''}</td>
        <td>${r.IcerikJson ? (r.IcerikJson.length > 30 ? r.IcerikJson.substring(0, 30) + "..." : r.IcerikJson) : ''}</td>
        <td>${r.Islenmis ? "✔️" : "❌"}</td>
        <td>${r.IslenmeTarihi ? new Date(r.IslenmeTarihi).toLocaleString() : ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await OutboxMesajApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmTip.value = r.Tip ?? '';
        fmIcerikJson.value = r.IcerikJson ?? '';
        fmIslenmis.checked = r.Islenmis ?? false;
        fmIslenmeTarihi.value = r.IslenmeTarihi ? new Date(r.IslenmeTarihi).toISOString().substring(0, 16) : '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await OutboxMesajApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
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
      Tip: val(fmTip),
      IcerikJson: val(fmIcerikJson),
      Islenmis: fmIslenmis.checked,
      IslenmeTarihi: fmIslenmeTarihi.value ? new Date(fmIslenmeTarihi.value).toISOString() : null
    };

    if (dto.Id) {
      await OutboxMesajApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await OutboxMesajApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
