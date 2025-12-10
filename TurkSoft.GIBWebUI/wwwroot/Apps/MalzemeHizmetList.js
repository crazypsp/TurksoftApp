// wwwroot/apps/MalzemeHizmetList.js
// View’da şöyle çağrılıyor:
// <script src="~/apps/listdata.js"></script>
// <script type="module" src="~/apps/MalzemeHizmetList.js"></script>

import { ItemApi, BrandApi, UnitApi } from '../Entites/index.js';

(function (window, $) {
    'use strict';

    if (typeof $ === 'undefined') {
        console.error('[MalzemeHizmetList] jQuery bulunamadı.');
        return;
    }

    /************************************************************
     *  ListData (unit & currency)
     ************************************************************/
    const ListData = window.ListData || {};
    const birimList = ListData.birimList || [];
    const parabirimList = ListData.parabirimList || [];

    /************************************************************
     *  RowVersion / BaseEntity yardımcıları
     ************************************************************/
    function rowVersionHexToBase64(hex) {
        if (!hex) return '';
        let h = String(hex).trim();
        if (h.startsWith('0x') || h.startsWith('0X')) {
            h = h.substring(2);
        }
        if (h.length % 2 === 1) {
            h = '0' + h;
        }
        const bytes = [];
        for (let i = 0; i < h.length; i += 2) {
            const b = parseInt(h.substr(i, 2), 16);
            if (!isNaN(b)) bytes.push(b);
        }
        let bin = '';
        for (let i = 0; i < bytes.length; i++) {
            bin += String.fromCharCode(bytes[i]);
        }
        try {
            return btoa(bin);
        } catch (e) {
            console.error('RowVersion base64 dönüşümünde hata:', e);
            return '';
        }
    }

    const DEFAULT_ROWVERSION_HEX = '0x00000000000007E1';
    const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

    function getRowVersionFromEntityOrDefault(entity) {
        if (!entity) return DEFAULT_ROWVERSION_BASE64;

        let rv =
            entity.rowVersionBase64 || entity.RowVersionBase64 ||
            entity.rowVersion || entity.RowVersion ||
            entity.rowVersionHex || entity.RowVersionHex;

        if (rv && /^0x/i.test(rv)) {
            rv = rowVersionHexToBase64(rv);
        }

        if (!rv) {
            rv = DEFAULT_ROWVERSION_BASE64;
        }
        return rv;
    }

    /**
     * Item için BaseEntity builder (swagger camelCase)
     */
    function buildBaseEntityForItem(isNew, nowIso, existing, overrideRowVersion) {
        const ex = existing || {};
        const BASE_USER_ID = getCurrentUserIdForGib(); // her zaman güncel kullanıcı

        if (isNew) {
            return {
                userId: BASE_USER_ID,
                isActive: true,
                deleteDate: null,
                deletedByUserId: null,
                createdAt: nowIso,
                updatedAt: nowIso,
                createdByUserId: BASE_USER_ID,
                updatedByUserId: BASE_USER_ID,
                rowVersion: overrideRowVersion || DEFAULT_ROWVERSION_BASE64
            };
        }

        const createdAt = ex.createdAt || ex.CreatedAt || nowIso;

        const isActive =
            (ex.isActive !== undefined ? ex.isActive :
                (ex.IsActive !== undefined ? ex.IsActive : true));

        const deleteDate = ex.deleteDate || ex.DeleteDate || null;
        const deletedByUserId = ex.deletedByUserId || ex.DeletedByUserId || null;

        const createdByUserId =
            ex.createdByUserId || ex.CreatedByUserId || BASE_USER_ID;

        const updatedByUserId =
            ex.updatedByUserId || ex.UpdatedByUserId || BASE_USER_ID;

        const userId =
            ex.userId || ex.UserId || BASE_USER_ID;

        let rowVersionBase64 = overrideRowVersion;
        if (!rowVersionBase64) {
            rowVersionBase64 = getRowVersionFromEntityOrDefault(ex);
        }

        return {
            userId: userId,
            isActive: isActive,
            deleteDate: deleteDate,
            deletedByUserId: deletedByUserId,
            createdAt: createdAt,
            updatedAt: nowIso,
            createdByUserId: createdByUserId,
            updatedByUserId: updatedByUserId,
            rowVersion: rowVersionBase64
        };
    }

    /************************************************************
     *  Küçük yardımcılar
     ************************************************************/
    function html(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /************************************************************
     *  Global state
     ************************************************************/
    const API = {
        exportUrl: '/stok/export',
        importUrl: '/stok/import-excel'
    };

    let table = null;              // DataTable instance
    let rows = [];                 // normalize edilmiş grid satırları
    let rawItemsById = {};         // id → API’den gelen entity
    let currentItemEntity = null;  // edit modunda seçili Item

    /************************************************************
     *  API → GRID normalize (Item JSON → grid modeli)
     ************************************************************/
    function normalizeApiItem(raw) {
        raw = raw || {};

        const id = (raw.id != null) ? raw.id : raw.Id;
        const code = (raw.code != null) ? raw.code : raw.Code;
        const name = (raw.name != null) ? raw.name : raw.Name;
        const price = (raw.price != null) ? raw.price : raw.Price;
        const currency = (raw.currency != null) ? raw.currency : raw.Currency || 'TRY';

        const brandId = (raw.brandId != null) ? raw.brandId : raw.BrandId;
        const brandName =
            (raw.brand && (raw.brand.name || raw.brand.Name)) ||
            (raw.Brand && (raw.Brand.name || raw.Brand.Name)) ||
            '';

        const unitId = (raw.unitId != null) ? raw.unitId : raw.UnitId;
        const unitShortName =
            (raw.unit && (raw.unit.shortName || raw.unit.ShortName)) ||
            (raw.Unit && (raw.Unit.shortName || raw.Unit.ShortName)) ||
            '';
        const unitName =
            (raw.unit && (raw.unit.name || raw.unit.Name)) ||
            (raw.Unit && (raw.Unit.name || raw.Unit.Name)) ||
            '';

        const userId = (raw.userId != null) ? raw.userId : raw.UserId;

        return {
            Id: id || 0,
            UserId: userId || 0,
            Code: code || '',
            Name: name || '',
            BrandId: brandId || null,
            BrandName: brandName || '',
            UnitId: unitId || null,
            UnitShortName: unitShortName || '',
            UnitName: unitName || '',
            Price: price != null ? price : null,
            Currency: currency || 'TRY'
        };
    }

    /************************************************************
     *  Lookups (Unit + Currency)
     ************************************************************/
    function initLookups() {
        // Birim
        const $unit = $('#mdlUnit');
        if ($unit.length) {
            const first = $unit.find('option').first();
            $unit.empty();
            if (first && first.val() === '') {
                $unit.append(first);
            }
            (birimList || []).forEach(b => {
                $unit.append(
                    $('<option/>', {
                        value: b.BirimId, // dropdown için internal id
                        text: `${b.Aciklama} - ${b.BirimKodu}`
                    })
                );
            });
        }

        // Para birimi
        const $cur = $('#mdlCurrency');
        if ($cur.length) {
            const first = $cur.find('option').first();
            $cur.empty();
            if (first && first.val() === '') {
                $cur.append(first);
            }
            (parabirimList || []).forEach(p => {
                $cur.append(
                    $('<option/>', {
                        value: p.Kodu,
                        text: `${p.Aciklama} (${p.Kodu})`
                    })
                );
            });
        }
    }

    /************************************************************
     *  DataTable
     ************************************************************/
    function initTable() {
        const $tbl = $('#gridStok');
        if (!$tbl.length) {
            console.warn('[MalzemeHizmetList] #gridStok tablo bulunamadı.');
            return;
        }

        table = $tbl.DataTable({
            data: [],
            pageLength: 25,
            order: [[1, 'asc']],
            columns: [
                {
                    data: 'Id',
                    orderable: false,
                    className: 'text-center',
                    render: function (id) {
                        return `<input type="checkbox" class="row-check" data-id="${html(id)}">`;
                    }
                },
                { data: 'Code' },          // Ürün/Hizmet Kodu
                { data: 'Name' },          // Ürün/Hizmet Adı
                { data: 'BrandName' },     // Marka
                {
                    data: 'UnitShortName', // Birim
                    render: function (v, type, row) {
                        return html(v || row.UnitName || '');
                    }
                },
                {
                    data: 'Price',         // Birim Fiyat
                    className: 'text-right',
                    render: function (v) {
                        if (v == null || v === '') return '';
                        const num = parseFloat(v);
                        if (isNaN(num)) return html(String(v));
                        return num.toLocaleString('tr-TR', {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 4
                        });
                    }
                },
                { data: 'Currency' },      // Para Birimi
                {
                    data: null,
                    orderable: false,
                    className: 'text-center',
                    render: function (row) {
                        return '' +
                            `<button class="btn btn-xs btn-primary act-edit" data-id="${html(row.Id)}"><i class="fa fa-edit"></i></button> ` +
                            `<button class="btn btn-xs btn-danger act-del" data-id="${html(row.Id)}"><i class="fa fa-trash"></i></button>`;
                    }
                }
            ],
            language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json' }
        });
    }

    function refreshTable() {
        if (!table) return;
        table.clear().rows.add(rows).draw();
    }

    /************************************************************
     *  API'den liste (UserId filtresi ile)
     ************************************************************/
    async function loadItems() {
        try {
            const userId = getCurrentUserIdForGib();
            const data = await ItemApi.list(); // backend GetAll tarafında da UserId koşulu olacak

            rawItemsById = {};
            const filtered = (data || []).filter(x => {
                const u = (x.userId != null) ? x.userId : x.UserId;
                return !userId || u === userId;
            });

            filtered.forEach(it => {
                const id = (it.id != null) ? it.id : it.Id;
                if (id != null) {
                    rawItemsById[id] = it;
                }
            });

            rows = filtered.map(normalizeApiItem);
            refreshTable();
        } catch (err) {
            console.error('[MalzemeHizmetList] listeleme hatası:', err);
            alert('Malzeme/Hizmet listesi alınamadı.');
        }
    }

    /************************************************************
     *  Modal açma
     ************************************************************/
    function openModal(entity) {
        const row = normalizeApiItem(entity || {});
        const $form = $('#UrunKayit');

        if ($form.length && $form[0].reset) {
            $form[0].reset();
        }

        $('#mdlId').val(row.Id || 0);
        $('#mdlCode').val(row.Code || '');
        $('#mdlName').val(row.Name || '');
        $('#mdlBrand').val(row.BrandName || '');

        const $unit = $('#mdlUnit');
        if ($unit.length) {
            if (row.UnitId) {
                $unit.val(row.UnitId);
            } else if (row.UnitShortName) {
                const match = birimList.find(b => b.BirimKodu === row.UnitShortName);
                if (match) $unit.val(match.BirimId);
                else $unit.val('');
            } else {
                $unit.val('');
            }
        }

        $('#mdlCurrency').val(row.Currency || 'TRY');
        $('#mdlPrice').val(row.Price != null ? String(row.Price) : '');

        $('#malzemeModal').modal('show');
    }

    /************************************************************
     *  Kaydet (create / update) – swagger Item JSON formatına göre
     ************************************************************/
    async function saveFromModal() {
        const form = $('#UrunKayit')[0];
        if (form && form.checkValidity && !form.checkValidity()) {
            form.reportValidity && form.reportValidity();
            return;
        }

        const id = parseInt($('#mdlId').val(), 10) || 0;
        const isNew = !id;

        const code = $('#mdlCode').val().trim();
        const name = $('#mdlName').val().trim();
        const brandName = $('#mdlBrand').val().trim();
        const unitIdStr = $('#mdlUnit').val();
        const priceVal = $('#mdlPrice').val();
        const price = priceVal ? parseFloat(String(priceVal).replace(',', '.')) : 0;
        const currency = $('#mdlCurrency').val() || 'TRY';

        if (!code) {
            alert('Ürün/Hizmet Kodu zorunludur.');
            return;
        }
        if (!name) {
            alert('Ürün/Hizmet Adı zorunludur.');
            return;
        }
        if (!unitIdStr) {
            alert('Ürün/Hizmet Birimi seçilmelidir.');
            return;
        }

        const nowIso = new Date().toISOString();
        const baseEntity = buildBaseEntityForItem(
            isNew,
            nowIso,
            isNew ? null : currentItemEntity,
            null
        );

        // Birim açıklamasını yakalayalım (Unit.name / Unit.shortName için)
        const selectedUnitMeta = birimList.find(
            b => String(b.BirimId) === String(unitIdStr)
        );
        const unitShort = selectedUnitMeta ? selectedUnitMeta.BirimKodu : 'C62';
        const unitName = selectedUnitMeta ? selectedUnitMeta.Aciklama : 'ADET';

        // Ortak alanlar
        const dtoBase = {
            ...baseEntity,
            id: isNew ? 0 : id,
            name: name,
            code: code,
            price: isNaN(price) ? 0 : price,
            currency: currency
        };

        let dto;

        if (isNew) {
            // YENİ KAYIT:
            // - unitId: 0 (EF, nested unit üzerinden set edecek)
            // - brand, unit nested objeler ile gönderilir
            dto = {
                ...dtoBase,
                brandId: 0,
                unitId: 0,

                brand: brandName
                    ? {
                        userId: baseEntity.userId,
                        isActive: true,
                        deleteDate: null,
                        deletedByUserId: null,
                        createdAt: baseEntity.createdAt,
                        updatedAt: baseEntity.updatedAt,
                        createdByUserId: baseEntity.createdByUserId,
                        updatedByUserId: baseEntity.updatedByUserId,
                        rowVersion: baseEntity.rowVersion,

                        id: 0,
                        name: brandName,
                        country: 'TR',
                        items: []
                    }
                    : null,

                unit: {
                    userId: baseEntity.userId,
                    isActive: true,
                    deleteDate: null,
                    deletedByUserId: null,
                    createdAt: baseEntity.createdAt,
                    updatedAt: baseEntity.updatedAt,
                    createdByUserId: baseEntity.createdByUserId,
                    updatedByUserId: baseEntity.updatedByUserId,
                    rowVersion: baseEntity.rowVersion,

                    id: 0,
                    name: unitName,
                    shortName: unitShort,
                    items: []
                },

                itemsCategories: [],
                itemsDiscounts: [],
                identifiers: []
            };
        } else {
            // GÜNCELLEME:
            // - Mevcut brandId / unitId’yi koruyoruz.
            const existingBrandId =
                currentItemEntity
                    ? (currentItemEntity.brandId ?? currentItemEntity.BrandId ?? 0)
                    : 0;

            const existingUnitId =
                currentItemEntity
                    ? (currentItemEntity.unitId ?? currentItemEntity.UnitId ?? 0)
                    : 0;

            dto = {
                ...dtoBase,
                brandId: existingBrandId,
                unitId: existingUnitId,

                brand: null,
                unit: null,
                itemsCategories: [],
                itemsDiscounts: [],
                identifiers: []
            };
        }

        try {
            if (isNew) {
                await ItemApi.create(dto);       // POST /api/v1/GibItem
            } else {
                await ItemApi.update(id, dto);   // PUT /api/v1/GibItem/{id}
            }

            $('#malzemeModal').modal('hide');
            await loadItems();
        } catch (err) {
            console.error('[MalzemeHizmetList] kaydetme hatası:', err);

            if (err && err.responseText) {
                console.log('API Error:', err.responseText);
            }

            alert('Kayıt kaydedilemedi.');
        }
    }

    /************************************************************
     *  Event binding
     ************************************************************/
    function bindEvents() {

        // Arama
        $('#btnAra').on('click', function () {
            if (!table) return;
            table.column(1).search($('#txtUrunKodu').val().trim());
            table.column(2).search($('#txtUrunHizmetAdi').val().trim()).draw();
        });

        $('#btnTemizle').on('click', function () {
            $('#txtUrunKodu,#txtUrunHizmetAdi').val('');
            if (table) {
                table.columns().search('').draw();
            }
        });

        // Tümünü seç
        $('#chkAll').on('change', function () {
            const ch = $(this).is(':checked');
            $('#gridStok tbody .row-check').prop('checked', ch);
        });

        // Yeni
        $('#btnMalzemeOlustur').on('click', function () {
            currentItemEntity = null;
            openModal(null);
        });

        // Kaydet
        $('#StokSave').on('click', function () {
            saveFromModal();
        });

        // Edit
        $(document).on('click', '.act-edit', async function () {
            const id = parseInt($(this).data('id'), 10);
            if (!id) return;

            try {
                const entity = await ItemApi.get(id);
                currentItemEntity = entity;
                openModal(entity);
            } catch (err) {
                console.error('[MalzemeHizmetList] get hatası:', err);
                alert('Kayıt getirilemedi.');
            }
        });

        // Sil
        $(document).on('click', '.act-del', async function () {
            const id = parseInt($(this).data('id'), 10);
            if (!id) return;
            if (!confirm('Kayıt silinsin mi?')) return;

            try {
                await ItemApi.remove(id);
                await loadItems();
            } catch (err) {
                console.error('[MalzemeHizmetList] silme hatası:', err);
                alert('Kayıt silinemedi.');
            }
        });

        // Toplu sil
        $('#btnTopluSil').on('click', async function () {
            const ids = [];
            $('#gridStok tbody .row-check:checked').each(function () {
                ids.push(parseInt($(this).data('id'), 10));
            });
            if (ids.length === 0) { alert('Seçim yok.'); return; }
            if (!confirm(ids.length + ' kayıt silinsin mi?')) return;

            try {
                for (let i = 0; i < ids.length; i++) {
                    await ItemApi.remove(ids[i]);
                }
                await loadItems();
                $('#chkAll').prop('checked', false);
            } catch (err) {
                console.error('[MalzemeHizmetList] toplu silme hatası:', err);
                alert('Silme işlemi sırasında hata oluştu.');
            }
        });

        // Excel'e Aktar
        $('#btnExceleAktar').on('click', function () {
            const q = $.param({
                Kod: $('#txtUrunKodu').val() || '',
                Ad: $('#txtUrunHizmetAdi').val() || ''
            });
            if (API.exportUrl) window.location = API.exportUrl + '?' + q;
            else alert('Export URL tanımlı değil (API.exportUrl).');
        });

        // Excel'den Aktar
        $('#FileExcel').on('change', function (e) {
            const file = e.target.files && e.target.files[0];
            if (!file) { return; }
            if (!API.importUrl) { alert('Import URL tanımlı değil (API.importUrl).'); this.value = ''; return; }

            const fd = new FormData();
            fd.append('file', file);
            $.ajax({
                url: API.importUrl,
                method: 'POST',
                data: fd,
                processData: false,
                contentType: false
            }).done(function () {
                alert('Excel yükleme tamamlandı.');
            }).fail(function (xhr) {
                alert('Excel yüklemede hata: ' + (xhr.responseText || xhr.statusText));
            }).always(function () {
                $('#FileExcel').val('');
            });
        });
    }

    /************************************************************
     *  UserId helper
     ************************************************************/
    function getCurrentUserIdForGib() {
        let userId = 0;

        // 1) Hidden input
        const $userHidden = $('#hdnUserId');
        if ($userHidden.length) {
            const parsed = parseInt($userHidden.val(), 10);
            if (!isNaN(parsed) && parsed > 0) {
                userId = parsed;
            }
        }

        // 2) window.currentUserId
        if (!userId && typeof window.currentUserId === 'number' && window.currentUserId > 0) {
            userId = window.currentUserId;
        }

        // 3) sessionStorage
        if (!userId && typeof sessionStorage !== 'undefined') {
            try {
                const stored =
                    sessionStorage.getItem('CurrentUserId') ||
                    sessionStorage.getItem('currentUserId') ||
                    sessionStorage.getItem('UserId');

                if (stored) {
                    const parsed = parseInt(stored, 10);
                    if (!isNaN(parsed) && parsed > 0) {
                        userId = parsed;
                        console.log('[Malzeme List] userId sessionStorage\'dan okundu:', userId);
                    }
                }
            } catch (e) {
                console.warn('[Malzeme List] CurrentUserId sessionStorage\'dan okunamadı:', e);
            }
        }

        if (!userId) {
            throw new Error('Kullanıcı Id (userId) bulunamadı. Lütfen oturumu kontrol edin.');
        }

        return userId;
    }

    /************************************************************
     *  INIT
     ************************************************************/
    $(function () {
        if (!$('#gridStok').length) {
            return;
        }

        try {
            getCurrentUserIdForGib();
        } catch (e) {
            console.error('[MalzemeHizmetList] userId alınamadı:', e);
            alert('Kullanıcı oturumu bulunamadı (userId).');
            return;
        }

        initLookups();
        initTable();
        bindEvents();
        loadItems();
    });

})(window, jQuery);
