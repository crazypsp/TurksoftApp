import { VergiOraniApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-vergiorani-body');
  const btnNew = $('#btnNewVergiOrani');
  const modalEl = $('#mdlVergiOrani');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmVergiOrani');

  // Form alanları
  const fmId = $('#frmId');
  const fmKod = $('#frmKod');
  const fmOran = $('#frmOran');

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
        Islem: `${userId}-VergiOrani-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydı başarısız:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await VergiOraniApi.list();
    await logIslem("List");

    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.Kod ?? ''}</td>
        <td>${r.Oran ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');

    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await VergiOraniApi.get(id);
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
        fmKod.value = r.Kod ?? '';
        fmOran.value = r.Oran ?? '';

        modal?.show();
      })
    );

    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await VergiOraniApi.remove(id);
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

  // ---- Insert / Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    const dto = {
      Id: fmId.value || undefined,
      Kod: val(fmKod),
      Oran: parseFloat(val(fmOran)) || 0
    };

    if (dto.Id) {
      await VergiOraniApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await VergiOraniApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
