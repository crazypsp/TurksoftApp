/* wwwroot/apps/invoice.js (UNIFIED — jQuery only)
 * - DATA BLOĞU AYNEN KORUNUR
 * - Satır ekle/sil (#manuel_grid), canlı toplam, taslak kaydet/yükle
 * - Önizleme (modal-onizleme / onizle-iframe)
 * - API: InvoiceApi (global) / REST fallback
 */

// Eğer modül import kullanıyorsan açabilirsin:
// import { InvoiceApi } from '../Entites/index.js';




// ----------------- yardımcılar -----------------
// invoice.js  (type="module" olsa da global jQuery ile çalışır)
(function ($) {
    "use strict";

    /* ==================== GENEL AYARLAR & YARDIMCILAR ==================== */

    const DRAFT_KEY = "einvoice_draft_v1";
    const VAT_MODE = { EXCL: "EXCL", INCL: "INCL" }; // Hariç / Dahil
    const DEFAULT_UNIT = { code: "C62", name: "ADET" };

    // --- Sayı parse & format ---
    function dec(v) {
        if (v == null) return 0;
        v = ("" + v).replace(" TL", "").trim();
        v = v.replace(/\./g, "").replace(",", ".");
        const n = parseFloat(v);
        return Number.isFinite(n) ? n : 0;
    }
    function fmt(n) {
        n = Math.round((n || 0) * 100) / 100;
        let s = n.toFixed(2);         // "1234.50"
        let [i, d] = s.split(".");
        i = i.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
        return i + "," + d;           // "1.234,50"
    }

    // --- UUID v4 ---
    function uuidv4() {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16).toUpperCase();
        });
    }

    function trySelect2($el) {
        try {
            if ($.fn.select2 && $el && $el.length) {
                $el.filter("select").select2({ width: "100%" });
            }
        } catch { }
    }

    // basit select doldurma
    function bindSelect($sel, list, valueField, textField, placeholder) {
        if (!$sel || !$sel.length || !Array.isArray(list)) return;

        const html = [];
        if (placeholder !== false) {
            html.push('<option value="">' + (placeholder || "Seçiniz") + "</option>");
        }

        list.forEach(function (x) {
            if (typeof x !== "object") {
                html.push('<option value="' + x + '">' + x + "</option>");
            } else {
                const v = x[valueField] ?? "";
                const t = x[textField] ?? v;
                html.push('<option value="' + v + '">' + t + "</option>");
            }
        });

        $sel.html(html.join(""));
    }

    /* ==================== 3) SELECTLERİ DATA BLOĞUNDAN DOLDURMA ==================== */

    // Buradaki list isimlerini Data bloğundakilerle eşleştir
    const SELECT_MAP = [
        { selector: "#subeKodu", list: "subeList", value: "SubeKodu", text: "SubeAdi" },
        { selector: "#SourceUrn", list: "GondericiEtiketList", value: null, text: null },
        { selector: "#DestinationUrn", list: "KurumEtiketList", value: null, text: null },
        { selector: "#DocumentCurrencyCode", list: "parabirimList", value: "Kodu", text: "Aciklama" },
        { selector: "#PayeeFinancialCurrencyCode", list: "parabirimList", value: "Kodu", text: "Aciklama" },
        { selector: 'select[name="txtCurrencyCode"]', list: "parabirimList", value: "Kodu", text: "Aciklama" },
        { selector: "#selulke", list: "ulkeList", value: "UlkeKodu", text: "UlkeAdi" },
        { selector: "#selIl", list: "ilList", value: "IlAdi", text: "IlAdi" },
        { selector: "#PaymentMeansCode", list: "OdemeList", value: "OdemeKodu", text: "Aciklama" },
        { selector: "#PaymentChannelCode", list: "OdemeKanalList", value: "OdemeKanalKodu", text: "Aciklama" },
        { selector: "#taxRepresentativeEtiket", list: "KurumEtiketList", value: null, text: null },
        // Satır birim / istisna seçiciler için ayrı fonksiyon var
    ];

    function fillStaticSelectsFromData() {
        SELECT_MAP.forEach(function (m) {
            const $sel = $(m.selector);
            if (!$sel.length) return;
            const list = window[m.list];
            if (!Array.isArray(list) || !list.length) return;
            bindSelect($sel, list, m.value ?? Object.keys(list[0])[0], m.text ?? Object.keys(list[0])[1], "Seçiniz");
            trySelect2($sel);
        });
    }

    // Satırdaki birim ve istisna selectleri
    function fillLineUnitSelect($scope) {
        const $targets = ($scope || $("#manuel_grid")).find(".ln-unit");
        const list = window.birimList;
        if (!Array.isArray(list) || !$targets.length) return;

        $targets.each(function () {
            const $this = $(this);
            const current = $this.val();
            bindSelect($this, list, "BirimKodu", "Aciklama", "Seçiniz");
            if (current) $this.val(current);
            else $this.val(DEFAULT_UNIT.code);
        });
    }

    function fillIstisnaSelect($sel) {
        const list = window.istisnaList;
        if (!Array.isArray(list) || !$sel.length) return;
        bindSelect($sel, list, "Kodu", "Adi", "Seçiniz");
    }

    /* ==================== 4) UUID OLUŞTURMA & KİLİTLEME ==================== */

    function initUUID() {
        const $uuid = $("#txtUUID");
        if (!$uuid.length) return;

        const newId = uuidv4();
        $uuid
            .val(newId)
            .prop("readonly", true)
            .css("user-select", "none")
            .on("keydown paste drop", function (e) { e.preventDefault(); })
            .on("contextmenu", function (e) { e.preventDefault(); });

        if (window.invoicemodel && invoicemodel.invoiceheader) {
            invoicemodel.invoiceheader.UUID = newId;
        }
    }

    /* ==================== 5) İSTİSNA KOLONU DAVRANIŞI ==================== */

    function isIstisnaFatura() {
        return ($("#ddlfaturatip").val() || "").toUpperCase() === "ISTISNA";
    }

    function setupIstisnaCell($row, active) {
        // İstisna sütunu: 0-based index: 10. kolon
        const $cell = $row.children("td").eq(10);
        if (!$cell.length) return;

        if (active) {
            // select'e çevir
            let $sel = $cell.find("select.ln-istisna");
            if (!$sel.length) {
                const prevVal = $cell.find("input.ln-istisna").val();
                $cell.empty();
                $sel = $('<select class="form-control ln-istisna"></select>');
                $cell.append($sel);
                fillIstisnaSelect($sel);
                if (prevVal) $sel.val(prevVal);
            } else {
                fillIstisnaSelect($sel);
            }
            $sel.prop("disabled", false);
        } else {
            // readonly input'a çevir
            let $inp = $cell.find("input.ln-istisna");
            if (!$inp.length) {
                const val = ($cell.find("select.ln-istisna").val() || "0,00");
                $cell.empty();
                $inp = $('<input class="form-control ln-istisna" type="text" />');
                $inp.val(val);
                $cell.append($inp);
            }
            $inp.prop("readonly", true);
        }
    }

    function applyFaturaTipiIstisnaMode() {
        const active = isIstisnaFatura();
        $("#manuel_grid tbody tr").each(function () {
            setupIstisnaCell($(this), active);
        });
    }

    /* ==================== 6) MANUEL_GRID HESAPLAMA / SATIR İŞLEMLERİ ==================== */

    function getVatMode() {
        // #kdvStatu : "false"=Hariç, "true"=Dahil
        const v = $("#kdvStatu").val();
        return v === "true" ? VAT_MODE.INCL : VAT_MODE.EXCL;
    }

    function renumberLines() {
        $("#manuel_grid tbody tr").each(function (i) {
            $(this).find(".ln-ix").text(i + 1);
        });
    }

    function addNewLineRow(afterRow) {
        const $tbody = $("#manuel_grid tbody");
        let $template = $tbody.find("tr:first");
        if (!$template.length) return;

        let $new = $template.clone(true, true);

        // inputları sıfırla
        $new.find("input").each(function () {
            const $i = $(this);
            if ($i.hasClass("ln-name")) {
                $i.val("");
            } else if ($i.hasClass("ln-qty") || $i.hasClass("ln-price") || $i.hasClass("ln-discp")) {
                $i.val(0);
            } else if ($i.hasClass("ln-isk") || $i.hasClass("ln-net") || $i.hasClass("ln-kdvt")) {
                $i.val("0,00");
            } else if ($i.hasClass("ln-kdv")) {
                $i.val(0);
            } else if ($i.hasClass("ln-istisna")) {
                $i.val("0,00");
            } else {
                // diğerleri
                $i.val("");
            }
        });

        // birim select doldurulmuş olsun
        fillLineUnitSelect($new);

        if (afterRow && afterRow.length) {
            $new.insertAfter(afterRow);
        } else {
            $tbody.append($new);
        }

        // fatura tipi -> istisna sütunu tipi
        setupIstisnaCell($new, isIstisnaFatura());

        renumberLines();
        recalcTotals();
    }

    function recalcTotals() {
        const mode = getVatMode();
        const autoMode = ($("#manuelToplam").val() || "false") === "false";

        let lineTotalBeforeDisc = 0;  // Mal/Hizmet toplam (iskonto öncesi)
        let totalDisc = 0;
        let totalNet = 0;             // Matrah
        let totalVat = 0;
        let totalGross = 0;

        $("#manuel_grid tbody tr").each(function () {
            const $r = $(this);
            const qty = dec($r.find(".ln-qty").val());
            const price = dec($r.find(".ln-price").val());
            const discPerc = dec($r.find(".ln-discp").val());
            const vatPerc = dec($r.find(".ln-kdv").val());

            if (qty <= 0 || price <= 0) {
                // satır boş ise alanları temizle
                $r.find(".ln-isk").val("0,00");
                $r.find(".ln-net").val("0,00");
                $r.find(".ln-kdvt").val("0,00");
                return;
            }

            let lineBase; // KDV hariç, iskonto öncesi
            if (mode === VAT_MODE.INCL) {
                const unitNet = vatPerc > 0 ? (price / (1 + vatPerc / 100)) : price;
                lineBase = qty * unitNet;
            } else {
                lineBase = qty * price;
            }

            const discAmount = lineBase * (discPerc / 100);
            const baseAfterDisc = lineBase - discAmount;
            const vatAmount = baseAfterDisc * (vatPerc / 100);
            const lineGross = baseAfterDisc + vatAmount;

            // satır alanlarını yaz
            $r.find(".ln-isk").val(fmt(discAmount));
            $r.find(".ln-net").val(fmt(baseAfterDisc));
            $r.find(".ln-kdvt").val(fmt(vatAmount));

            lineTotalBeforeDisc += lineBase;
            totalDisc += discAmount;
            totalNet += baseAfterDisc;
            totalVat += vatAmount;
            totalGross += lineGross;
        });

        if (autoMode) {
            if ($("#tAra").length) $("#tAra").val(fmt(lineTotalBeforeDisc) + " TL");
            if ($("#tIsk").length) $("#tIsk").val(fmt(totalDisc) + " TL");
            if ($("#tMatrah20").length) $("#tMatrah20").val(fmt(totalNet) + " TL");
            if ($("#tKdv20").length) $("#tKdv20").val(fmt(totalVat) + " TL");
            if ($("#tVergiDahil").length) $("#tVergiDahil").val(fmt(totalGross) + " TL");
            if ($("#tGenel").length) $("#tGenel").val(fmt(totalGross) + " TL");

            // Döviz - TL kısmı (opsiyonel)
            updateTLTotals(lineTotalBeforeDisc, totalDisc, totalNet, totalVat, totalGross);
        }

        // invoicemodel varsa istenirse buraya da yazılabilir
        if (window.invoicemodel && invoicemodel.invoiceheader) {
            invoicemodel.invoiceheader.LineExtensionAmount = totalNet;
            invoicemodel.invoiceheader.AllowanceTotalAmount = totalDisc;
            invoicemodel.invoiceheader.TaxExclusiveAmount = totalNet;
            invoicemodel.invoiceheader.TaxInclusiveAmount = totalNet + totalVat;
            invoicemodel.invoiceheader.PayableAmount = totalGross;
        }
    }

    function updateTLTotals(ara, isk, matrah, kdv, genel) {
        const currency = ($("#DocumentCurrencyCode").val() || "TRY").toUpperCase();
        const kur = dec($("#txtKurBilgisi").val());
        const showTL = currency !== "TRY" && kur > 0;

        if (!showTL) {
            $("#tlTotals").hide();
            return;
        }
        const toTL = (x) => x * kur;

        $("#tlTotals").show();
        if ($("#tAraTL").length) $("#tAraTL").val(fmt(toTL(ara)) + " TL");
        if ($("#tIskTL").length) $("#tIskTL").val(fmt(toTL(isk)) + " TL");
        if ($("#tVergiDahilTL").length) $("#tVergiDahilTL").val(fmt(toTL(genel)) + " TL");
        if ($("#tGenelTL").length) $("#tGenelTL").val(fmt(toTL(genel)) + " TL");
        // İstersen matrah / KDV TL'lerini de ayrı ayrı hesaplayabilirsin
    }

    /* ==================== 2) TÜM "YENİ SATIR / YENİ İADE" BUTONLARI ==================== */

    // İrsaliye satırı
    function addDespatchRow() {
        const $tbody = $("#irsaliye_grid tbody");
        const $row = $(`
            <tr>
                <td><input type="text" class="form-control js-irs-no" /></td>
                <td><input type="text" class="form-control datepicker js-irs-date" /></td>
                <td style="text-align:center">
                    <button type="button" class="btn btn-danger btn-xs js-row-del">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>`);
        $tbody.append($row);
        if ($.fn.datepicker) $row.find(".datepicker").datepicker();
    }

    // İade Fatura satırı
    function addIadeRow() {
        const $tbody = $("#iade_grid tbody");
        const $row = $(`
            <tr>
                <td><input type="text" class="form-control js-iade-no" /></td>
                <td><input type="text" class="form-control datepicker js-iade-date" /></td>
                <td style="text-align:center">
                    <button type="button" class="btn btn-danger btn-xs js-row-del">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>`);
        $tbody.append($row);
        if ($.fn.datepicker) $row.find(".datepicker").datepicker();
    }

    // Ek alan tabloları (tanıtıcı kod / değer)
    function addExtraRow(tableId) {
        const $tbody = $("#" + tableId + " tbody");
        if (!$tbody.length) return;

        const $row = $(`
            <tr>
                <td>
                    <select class="form-control js-tanitici-kod"></select>
                </td>
                <td>
                    <input type="text" class="form-control js-tanitici-deger" />
                </td>
                <td style="text-align:center">
                    <button type="button" class="btn btn-danger btn-xs js-row-del">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>`);

        $tbody.append($row);

        const list = window.taniticikodList;
        if (Array.isArray(list)) {
            bindSelect($row.find(".js-tanitici-kod"), list, "TaniticiKod", "TaniticiKod", "Seçiniz");
        }
        trySelect2($row.find(".js-tanitici-kod"));
    }

    /* ==================== 7) TOPLAM BİLGİLERİ OTOMATİK / MANUEL ==================== */

    function toggleManualTotals() {
        const v = ($("#manuelToplam").val() || "false") === "true";
        // true => manuel açık
        $(".man-otm[data-mode='manual']").toggle(v);
        $(".man-otm[data-mode='auto']").toggle(!v);

        if (!v) {
            // otomatik moda geçince tekrar hesapla
            recalcTotals();
        }
    }

    /* ==================== 8) ALICIDAN KOPYALA ==================== */

    function copyBuyerAddressToKamu() {
        // Ülke
        const ulke = $("#txtulke").is(":visible") && !$("#txtulke").hasClass("hidden")
            ? $("#txtulke").val()
            : ($("#selulke").val() || $("#selulke option:selected").text() || "");

        // İl
        const il = $("#txtIl").is(":visible") && !$("#txtIl").hasClass("hidden")
            ? $("#txtIl").val()
            : ($("#selIl").val() || $("#selIl option:selected").text() || "");

        // İlçe
        const ilce = $("#txtIlce").is(":visible") && !$("#txtIlce").hasClass("hidden")
            ? $("#txtIlce").val()
            : ($("#selIlce").val() || $("#selIlce option:selected").text() || "");

        $("#txtKamuUlke").val(ulke);
        $("#IlbuyerCustomer").val(il);
        $("#taxRepresentativeCitySubdivisionName").val(ilce);
    }

    /* ==================== TASLAK KAYDET (LOCALSTORAGE) ==================== */

    function collectDraft() {
        const header = {
            subeKodu: $("#subeKodu").val(),
            SourceUrn: $("#SourceUrn").val(),
            UUID: $("#txtUUID").val(),
            Prefix: $("#txtInvoice_Prefix").val(),
            Senaryo: $("#ddlsenaryo").val(),
            FaturaTipi: $("#ddlfaturatip").val(),
            IssueDate: $("#txtIssueDate").val(),
            IssueTime: $("#txtIssueTime").val(),
            Currency: $("#DocumentCurrencyCode").val(),
            Kur: $("#txtKurBilgisi").val()
        };

        const buyer = {
            VknTckn: $("#txtIdentificationID").val(),
            PartyName: $("#txtPartyName").val(),
            FirstName: $("#txtPerson_FirstName").val(),
            FamilyName: $("#txtPerson_FamilyName").val(),
            TaxOffice: $("#txtTaxSchemeName").val(),
            Ulke: $("#selulke").val() || $("#txtulke").val(),
            Il: $("#selIl").val() || $("#txtIl").val(),
            Ilce: $("#selIlce").val() || $("#txtIlce").val(),
            Adres: $("#txtStreetName").val(),
            Email: $("#txtElectronicMail").val(),
            PostaKodu: $("#txtPostalZone").val(),
            Web: $("#txtWebsiteURI").val(),
            Telefon: $("#txtTelephone").val()
        };

        const kamu = {
            Vkn: $("#txtKamuVkn").val(),
            Unvan: $("#txtKamuUnvan").val(),
            Ulke: $("#txtKamuUlke").val(),
            Il: $("#IlbuyerCustomer").val(),
            Ilce: $("#taxRepresentativeCitySubdivisionName").val()
        };

        const lines = [];
        $("#manuel_grid tbody tr").each(function () {
            const $r = $(this);
            lines.push({
                name: $r.find(".ln-name").val(),
                qty: $r.find(".ln-qty").val(),
                unit: $r.find(".ln-unit").val(),
                price: $r.find(".ln-price").val(),
                discp: $r.find(".ln-discp").val(),
                isk: $r.find(".ln-isk").val(),
                net: $r.find(".ln-net").val(),
                kdvPerc: $r.find(".ln-kdv").val(),
                kdvTutar: $r.find(".ln-kdvt").val(),
                istisna: $r.find(".ln-istisna").val()
            });
        });

        const totals = {
            manuelToplam: $("#manuelToplam").val(),
            tAra: $("#tAra").val(),
            tIsk: $("#tIsk").val(),
            tMatrah20: $("#tMatrah20").val(),
            tKdv20: $("#tKdv20").val(),
            tVergiDahil: $("#tVergiDahil").val(),
            tGenel: $("#tGenel").val()
        };

        return { header, buyer, kamu, lines, totals };
    }

    function saveDraft() {
        try {
            const data = collectDraft();
            localStorage.setItem(DRAFT_KEY, JSON.stringify(data));
            if (window.toastr) toastr.success("Taslak kaydedildi.");
            console.log("Taslak kaydedildi:", data);
        } catch (e) {
            console.error("Taslak kaydedilemedi:", e);
            if (window.toastr) toastr.error("Taslak kaydedilirken hata oluştu.");
        }
    }

    function loadDraft() {
        try {
            const raw = localStorage.getItem(DRAFT_KEY);
            if (!raw) return;
            const d = JSON.parse(raw);

            if (d.header) {
                $("#subeKodu").val(d.header.subeKodu);
                $("#SourceUrn").val(d.header.SourceUrn);
                // UUID her sayfa açıldığında yeni üretiliyor; draft'takini kullanmıyoruz
                $("#txtInvoice_Prefix").val(d.header.Prefix);
                $("#ddlsenaryo").val(d.header.Senaryo);
                $("#ddlfaturatip").val(d.header.FaturaTipi);
                $("#txtIssueDate").val(d.header.IssueDate);
                $("#txtIssueTime").val(d.header.IssueTime);
                $("#DocumentCurrencyCode").val(d.header.Currency);
                $("#txtKurBilgisi").val(d.header.Kur);
            }
            if (d.buyer) {
                $("#txtIdentificationID").val(d.buyer.VknTckn);
                $("#txtPartyName").val(d.buyer.PartyName);
                $("#txtPerson_FirstName").val(d.buyer.FirstName);
                $("#txtPerson_FamilyName").val(d.buyer.FamilyName);
                $("#txtTaxSchemeName").val(d.buyer.TaxOffice);
                $("#txtStreetName").val(d.buyer.Adres);
                $("#txtElectronicMail").val(d.buyer.Email);
                $("#txtPostalZone").val(d.buyer.PostaKodu);
                $("#txtWebsiteURI").val(d.buyer.Web);
                $("#txtTelephone").val(d.buyer.Telefon);
            }
            if (d.kamu) {
                $("#txtKamuVkn").val(d.kamu.Vkn);
                $("#txtKamuUnvan").val(d.kamu.Unvan);
                $("#txtKamuUlke").val(d.kamu.Ulke);
                $("#IlbuyerCustomer").val(d.kamu.Il);
                $("#taxRepresentativeCitySubdivisionName").val(d.kamu.Ilce);
            }

            // satırları yeniden oluştur
            if (Array.isArray(d.lines) && d.lines.length) {
                const $tbody = $("#manuel_grid tbody");
                $tbody.empty();
                d.lines.forEach(function (ln, idx) {
                    const $row = $(`
                        <tr>
                            <td class="text-center ln-ix"></td>
                            <td><input class="form-control ln-name" type="text" /></td>
                            <td><input class="form-control ln-qty" step="0.0001" type="number" /></td>
                            <td><select class="form-control ln-unit"></select></td>
                            <td><input class="form-control ln-price" step="0.0001" type="number" /></td>
                            <td><input class="form-control ln-discp" step="0.01" type="number" /></td>
                            <td><input class="form-control ln-isk" type="text" /></td>
                            <td><input class="form-control ln-net" type="text" /></td>
                            <td><input class="form-control ln-kdv" step="0.01" type="number" /></td>
                            <td><input class="form-control ln-kdvt" type="text" /></td>
                            <td><input class="form-control ln-istisna" type="text" /></td>
                            <td class="text-center">
                                <div class="btn-group btn-group-xs einv-btn-group" role="group">
                                    <button class="btn btn-success js-line-add" type="button" title="Altına Satır Ekle"><i class="fa fa-plus"></i></button>
                                    <button class="btn btn-info js-line-edit" type="button" title="Düzenle"><i class="fa fa-pencil"></i></button>
                                    <button class="btn btn-danger js-line-del" type="button" title="Sil"><i class="fa fa-trash"></i></button>
                                </div>
                            </td>
                        </tr>`);
                    $row.find(".ln-name").val(ln.name);
                    $row.find(".ln-qty").val(ln.qty);
                    $row.find(".ln-price").val(ln.price);
                    $row.find(".ln-discp").val(ln.discp);
                    $row.find(".ln-isk").val(ln.isk);
                    $row.find(".ln-net").val(ln.net);
                    $row.find(".ln-kdv").val(ln.kdvPerc);
                    $row.find(".ln-kdvt").val(ln.kdvTutar);
                    $row.find(".ln-istisna").val(ln.istisna);
                    $tbody.append($row);
                });

                fillLineUnitSelect($tbody);
                applyFaturaTipiIstisnaMode();
                renumberLines();
            }

            if (d.totals) {
                $("#manuelToplam").val(d.totals.manuelToplam || "false");
                $("#tAra").val(d.totals.tAra);
                $("#tIsk").val(d.totals.tIsk);
                $("#tMatrah20").val(d.totals.tMatrah20);
                $("#tKdv20").val(d.totals.tKdv20);
                $("#tVergiDahil").val(d.totals.tVergiDahil);
                $("#tGenel").val(d.totals.tGenel);
            }

            toggleManualTotals();
            recalcTotals();
        } catch (e) {
            console.error("Taslak okunamadı:", e);
        }
    }

    /* ==================== DOM READY ==================== */

    $(function () {
        // 3) Tüm selectleri data bloktan doldur
        fillStaticSelectsFromData();
        fillLineUnitSelect();

        // 4) UUID üret ve inputu kilitle
        initUUID();

        // 2) Butonlar -> satır ekleme
        $("#btnYeniSatir").on("click", function () {
            addNewLineRow();
        });
        $("#manuel_grid").on("click", ".js-line-add", function () {
            const $row = $(this).closest("tr");
            addNewLineRow($row);
        });
        $("#manuel_grid").on("click", ".js-line-del", function () {
            const $row = $(this).closest("tr");
            $row.remove();
            if (!$("#manuel_grid tbody tr").length) {
                addNewLineRow();
            }
            renumberLines();
            recalcTotals();
        });
        $("#manuel_grid").on("click", ".js-line-edit", function () {
            $(this).closest("tr").find(".ln-name").focus().select();
        });

        $("#btnDespatchAdd").on("click", addDespatchRow);
        $("#btnIadeAdd").on("click", addIadeRow);
        $("#btnSellerExtraAdd").on("click", function () { addExtraRow("saticiekalan_grid"); });
        $("#btnSellerAgentExtraAdd").on("click", function () { addExtraRow("saticiAgentekalan_grid"); });
        $("#btnBuyerExtraAdd").on("click", function () { addExtraRow("aliciekalan_grid"); });

        // 6) Hesaplamayı tetikleyecek alanlar
        $(document).on("input change",
            "#kdvStatu, #DocumentCurrencyCode, #txtKurBilgisi, #manuel_grid .ln-qty, #manuel_grid .ln-price, #manuel_grid .ln-discp, #manuel_grid .ln-kdv",
            function () { recalcTotals(); });

        // 7) Otomatik / manuel toplam
        $("#manuelToplam").on("change", function () {
            toggleManualTotals();
            recalcTotals();
        });
        toggleManualTotals();

        // 5) Fatura tipi -> istisna sütunu
        $("#ddlfaturatip").on("change", function () {
            applyFaturaTipiIstisnaMode();
        });
        applyFaturaTipiIstisnaMode();

        // 8) Alıcıdan Kopyala
        $("#btnAlicidanKopyala").on("click", function () {
            copyBuyerAddressToKamu();
        });

        // Taslak Kaydet
        $("#btn_taslak").on("click", function () {
            saveDraft();
        });

        // (İstersen) sayfa açılınca taslağı yükle
        loadDraft();

        // İlk hesaplama
        renumberLines();
        recalcTotals();

        // Manuel şehir seçimi (checkbox) – cshtml’de vardı
        $('input[name="manuelSehir"]').on("change", function () {
            const on = $(this).is(":checked");
            const $c = $("#selIl, #selIlce");
            const $t = $("#txtIl, #txtIlce");

            if (on) {
                $c.closest(".inputPadding").find("select").addClass("hidden");
                $t.removeClass("hidden");
            } else {
                $c.closest(".inputPadding").find("select").removeClass("hidden");
                $t.addClass("hidden");
            }
        });
    });

})(jQuery);


