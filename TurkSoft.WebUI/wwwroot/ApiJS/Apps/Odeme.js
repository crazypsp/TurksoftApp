import { OdemeApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-odeme-body');
  const btnNew = $('#btnNewOdeme');
  const modalEl = $('#mdlOdeme');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmOdeme');

  // Form alanları
  const fmId = $('#frmId');
  const fmSatisId = $('#frmSatisId');
  const fmOdemeTarihi = $('#frmOdemeTarihi');
  const fmOdemeYontemi = $('#frmOdemeYontemi');
  const fmOdemeDurumu = $('#frmOdemeDurumu');
  const fmTutar = $('#frmTutar');
  const fmKomisyonOrani = $('#frmKomisyonOrani');
  const fmKomisyonTutar = $('#frmKomisyonTutar');
  const fmNetTutar = $('#frmNetTutar');
  const fmSanalPosId = $('#frmSanalPosId');
  const fmSaglayiciIslemNo = $('#frmSaglayiciIslemNo');
  const fmTaksit = $('#frmTaksit');
  const fmAciklama = $('#frmAciklama');

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
    const list = await OdemeApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.SatisId ?? ''}</td>
        <td>${r.OdemeTarihi ?? ''}</td>
        <td>${r.Tutar ?? ''}</td>
        <td>${r.NetTutar ?? ''}</td>
        <td>${r.OdemeDurumu ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await OdemeApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmSatisId.value = r.SatisId ?? '';
      fmOdemeTarihi.value = r.OdemeTarihi ?? '';
      fmOdemeYontemi.value = r.OdemeYontemi ?? 0;
      fmOdemeDurumu.value = r.OdemeDurumu ?? 0;
      fmTutar.value = r.Tutar ?? '';
      fmKomisyonOrani.value = r.KomisyonOrani ?? '';
      fmKomisyonTutar.value = r.KomisyonTutar ?? '';
      fmNetTutar.value = r.NetTutar ?? '';
      fmSanalPosId.value = r.SanalPosId ?? '';
      fmSaglayiciIslemNo.value = r.SaglayiciIslemNo ?? '';
      fmTaksit.value = r.Taksit ?? '';
      fmAciklama.value = r.Aciklama ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await OdemeApi.remove(id);
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
      SatisId: val(fmSatisId),
      OdemeTarihi: val(fmOdemeTarihi),
      OdemeYontemi: parseInt(val(fmOdemeYontemi)) || 0,
      OdemeDurumu: parseInt(val(fmOdemeDurumu)) || 0,
      Tutar: parseFloat(val(fmTutar)) || 0,
      KomisyonOrani: val(fmKomisyonOrani) ? parseFloat(val(fmKomisyonOrani)) : null,
      KomisyonTutar: val(fmKomisyonTutar) ? parseFloat(val(fmKomisyonTutar)) : null,
      NetTutar: parseFloat(val(fmNetTutar)) || 0,
      SanalPosId: val(fmSanalPosId) || null,
      SaglayiciIslemNo: val(fmSaglayiciIslemNo),
      Taksit: val(fmTaksit),
      Aciklama: val(fmAciklama)
    };

    if (dto.Id) {
      await OdemeApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await OdemeApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
