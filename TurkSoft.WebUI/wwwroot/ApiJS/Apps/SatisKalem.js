import { SatisKalemApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-satiskalem-body');
  const btnNew = $('#btnNewSatisKalem');
  const modalEl = $('#mdlSatisKalem');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmSatisKalem');

  // Form alanları
  const fmId = $('#frmId');
  const fmSatisId = $('#frmSatisId');
  const fmPaketId = $('#frmPaketId');
  const fmMiktar = $('#frmMiktar');
  const fmBirimFiyat = $('#frmBirimFiyat');
  const fmTutar = $('#frmTutar');

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
        Islem: `${userId}-SatisKalem-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log kaydedilemedi:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await SatisKalemApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.SatisId ?? ''}</td>
        <td>${r.PaketId ?? ''}</td>
        <td>${r.Miktar ?? ''}</td>
        <td>${r.BirimFiyat ?? ''}</td>
        <td>${r.Tutar ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del" data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await SatisKalemApi.get(id);
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
        fmSatisId.value = r.SatisId ?? '';
        fmPaketId.value = r.PaketId ?? '';
        fmMiktar.value = r.Miktar ?? '';
        fmBirimFiyat.value = r.BirimFiyat ?? '';
        fmTutar.value = r.Tutar ?? '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await SatisKalemApi.remove(id);
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
      SatisId: val(fmSatisId),
      PaketId: val(fmPaketId),
      Miktar: parseInt(val(fmMiktar)) || 0,
      BirimFiyat: parseFloat(val(fmBirimFiyat)) || 0,
      Tutar: parseFloat(val(fmTutar)) || 0
    };

    if (dto.Id) {
      await SatisKalemApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await SatisKalemApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
