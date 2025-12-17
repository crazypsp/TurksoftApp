// Login.js'teki gibi: Service'den create fonksiyonunu al
import { create as createInvoice } from '../entites/EArchiveInvoice.js';

; (function (global, $) {
    "use strict";

    /***********************************************************
     *  UTILS
     ***********************************************************/
    const Utils = {
        parseNumber: function (value) {
            if (value === null || value === undefined) return 0;
            let str = String(value).trim();
            if (str === "") return 0;
            str = str.replace(/\./g, "").replace(",", ".");
            const n = parseFloat(str);
            return isNaN(n) ? 0 : n;
        },
        formatNumber: function (value, decimals) {
            decimals = (decimals == null) ? 2 : decimals;
            const n = Number(value) || 0;
            return n.toLocaleString("tr-TR", {
                minimumFractionDigits: decimals,
                maximumFractionDigits: decimals
            });
        }
    };

    /***********************************************************
     *  RowVersion yardımcıları
     ***********************************************************/
    function rowVersionHexToBase64(hex) {
        if (!hex) return "";
        let h = String(hex).trim();
        if (h.startsWith("0x") || h.startsWith("0X")) h = h.substring(2);
        if (h.length % 2 === 1) h = "0" + h;

        const bytes = [];
        for (let i = 0; i < h.length; i += 2) {
            const b = parseInt(h.substr(i, 2), 16);
            if (!isNaN(b)) bytes.push(b);
        }
        let bin = "";
        for (let i = 0; i < bytes.length; i++) bin += String.fromCharCode(bytes[i]);

        try { return btoa(bin); }
        catch (e) {
            console.error("RowVersion base64 dönüşümünde hata:", e);
            return "";
        }
    }

    function buildBaseEntity(currentUserId, nowIso, rowVersionBase64) {
        return {
            UserId: currentUserId || 0,
            IsActive: true,
            DeleteDate: null,
            DeletedByUserId: null,
            CreatedAt: nowIso,
            UpdatedAt: null,
            CreatedByUserId: currentUserId || null,
            UpdatedByUserId: null,
            RowVersion: rowVersionBase64 || null
        };
    }

    /***********************************************************
     *  GİB / USERID yardımcıları
     ***********************************************************/
    function syncUserIdHiddenFromSession() {
        const $hdn = $("#hdnUserId");
        if (!$hdn.length) return;

        if (!$hdn.val()) {
            try {
                const stored =
                    sessionStorage.getItem("CurrentUserId") ||
                    sessionStorage.getItem("currentUserId") ||
                    sessionStorage.getItem("UserId");

                if (stored) $hdn.val(stored);
            } catch (e) {
                console.warn("[EArchive] CurrentUserId sessionStorage'dan okunamadı:", e);
            }
        }
    }

    function getCurrentUserIdForGib() {
        let userId = 0;

        const $userHidden = $("#hdnUserId");
        if ($userHidden.length) {
            const parsed = parseInt($userHidden.val(), 10);
            if (!isNaN(parsed) && parsed > 0) userId = parsed;
        }

        if (!userId && typeof global.currentUserId === "number" && global.currentUserId > 0) {
            userId = global.currentUserId;
        }

        if (!userId && typeof sessionStorage !== "undefined") {
            try {
                const stored =
                    sessionStorage.getItem("CurrentUserId") ||
                    sessionStorage.getItem("currentUserId") ||
                    sessionStorage.getItem("UserId");

                if (stored) {
                    const parsed = parseInt(stored, 10);
                    if (!isNaN(parsed) && parsed > 0) userId = parsed;
                }
            } catch (e) {
                console.warn("[EArchive] CurrentUserId sessionStorage'dan okunamadı:", e);
            }
        }

        if (!userId) throw new Error("Kullanıcı Id (userId) bulunamadı. Lütfen oturumu kontrol edin.");
        return userId;
    }

    function getGibBaseUrl() {
        return global.gibPortalEArchiveApiBaseUrl || global.gibPortalApiBaseUrl || null;
    }

    function getEArchiveRoutes() {
        // Override edilebilir:
        // window.gibEArchiveRoutes = { sendJson:"...", pdf:"...", ubl:"..." }
        return global.gibEArchiveRoutes || {
            sendJson: "TurkcellEArsiv/earchive/send-json/",
            pdf: "TurkcellEArsiv/earchive/outbox/pdf/",
            ubl: "TurkcellEArsiv/earchive/outbox/ubl/"
        };
    }

    /***********************************************************
     *  GİB PORTAL API – e-Arşiv Fatura Gönderimi
     ***********************************************************/
    async function sendEArchiveToGibById(invoiceId) {
        const numericId = Number(invoiceId);
        if (!Number.isFinite(numericId) || numericId <= 0) {
            throw new Error("Geçersiz invoiceId: " + invoiceId);
        }

        // e-Arşiv için öncelik: gibPortalEArchiveApiBaseUrl, yoksa gibPortalApiBaseUrl, yoksa same-origin
        const rawBase =
            window.gibPortalEArchiveApiBaseUrl ||
            window.gibPortalApiBaseUrl ||
            window.location.origin;

        function normalizeApiBaseUrl(baseUrl) {
            let b = (baseUrl || "").toString().trim();
            if (!b) return "";

            // slash garanti
            if (!b.endsWith("/")) b += "/";

            // baseUrl zaten /api/ içeriyor mu?
            const hasApiSegment = /\/api\/($|.*)/i.test(b);

            // yoksa /api/ ekle
            if (!hasApiSegment) b += "api/";

            return b;
        }

        const normBase = normalizeApiBaseUrl(rawBase);
        if (!normBase) {
            throw new Error("GİB Portal API Base URL çözülemedi.");
        }

        const userId = getCurrentUserIdForGib();

        const qs = new URLSearchParams({
            userId: String(userId)
        });

        // ✅ Controller: TurkcellEFaturaController  => /api/TurkcellEFatura/...
        const url =
            normBase +
            "TurkcellEFatura/earchive/send-json/" +
            encodeURIComponent(numericId) +
            "?" + qs.toString();

        console.log("[EArchive][GIB] Gönderim URL:", url);

        const response = await fetch(url, {
            method: "POST",
            headers: { "Accept": "application/json" }
        });

        const contentType = (response.headers.get("content-type") || "").toLowerCase();
        const isJson = contentType.includes("application/json");
        const rawText = await response.text();

        let data = null;
        if (isJson && rawText) {
            try { data = JSON.parse(rawText); }
            catch (e) { console.warn("[EArchive][GIB] JSON parse hatası:", e, rawText); }
        }

        if (!response.ok) {
            let warningMessages = [];

            if (data && typeof data === "object") {
                const uyarilarRaw =
                    data["uyarı"] || data["uyari"] || data["Uyarı"] || data["Uyari"] ||
                    data["warnings"] || data["warning"];

                if (Array.isArray(uyarilarRaw)) warningMessages = uyarilarRaw;
                else if (typeof uyarilarRaw === "string" && uyarilarRaw.trim()) warningMessages = [uyarilarRaw.trim()];
            }

            const msg =
                (warningMessages.length
                    ? warningMessages.join("\n")
                    : (data && (data.message || data.Message)) ||
                    `GİB API isteği başarısız. Status: ${response.status} ${response.statusText}`);

            const err = new Error(msg);
            err.status = response.status;
            err.payload = data;
            throw err;
        }

        if (!data && rawText) {
            try { data = JSON.parse(rawText); }
            catch { data = {}; }
        }

        return data || {};
    }


    /***********************************************************
     *  ListData
     ***********************************************************/
    global.ListData = global.ListData || {};
    function getListData() { return global.ListData || {}; }

    /***********************************************************
     *  UI helpers
     ***********************************************************/
    function generateUUIDv4() {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        }).toUpperCase();
    }

    function showAlert(type, message) {
        const $form = $("#faturaformu");
        if (!$form.length) return;

        $(".invoice-alert").remove();
        const $alert = $(`
            <div class="alert alert-${type} invoice-alert" role="alert" style="margin-bottom:15px;">
                ${message}
            </div>
        `);

        $form.prepend($alert);
        setTimeout(function () { $alert.fadeOut(400, function () { $(this).remove(); }); }, 5000);
    }

    /***********************************************************
     *  GLOBAL STATE + MODEL
     ***********************************************************/
    const UIState = { selectedCustomerId: null };

    const invoiceModel = {
        invoiceheader: {
            Invoice_ID: null,
            IssueDate: null,
            IssueTime: null,
            SourceUrn: null,
            Prefix: null,
            DocumentCurrencyCode: "TRY",
            Currency: "TRY",
            Note: null,
            LineExtensionAmount: 0,
            TaxInclusiveAmount: 0,
            PayableAmount: 0,
            Taxes: 0,
            InvoiceTypeCode: null,
            Scenario: null,
            BranchCode: null,
            TemplateName: null,

            // e-Arşiv özel
            EArchiveSendType: "KAGIT",
            IsInternetSale: false,
            IsInternet_WebsiteURI: null,
            IsInternet_ActualDespatchDate: null,
            IsInternet_Delivery_TcknVkn: null,
            IsInternet_Delivery_PartyName: null,
            IsInternet_Delivery_FirstName: null,
            IsInternet_Delivery_FamilyName: null
        },

        customer: null,   // preview için nested
        Customer: null,   // flat customer
        payment: null,
        irsaliyeler: [],
        sellerAdditional: [],
        sellerAgentAdditional: [],
        buyerAdditional: [],
        ekAlan: [],
        invoiceLines: []
    };

    global.invoiceModel = invoiceModel;
    global.Utils = Utils;
    global.UIState = UIState;

    /***********************************************************
     *  (Opsiyonel) Demo Customers
     ***********************************************************/
    const TestCustomers = [
        {
            id: 1,
            displayName: "0001 - TEST FİRMA A.Ş.",
            identificationId: "11111111111",
            partyName: "TEST FİRMA A.Ş.",
            firstName: "TEST",
            lastName: "FİRMA",
            taxSchemeName: "İSTANBUL VERGİ DAİRESİ",
            ulkeCode: "TR",
            ulkeText: "Türkiye",
            ilCode: "34",
            ilName: "İSTANBUL",
            ilceCode: "3401",
            ilceName: "KADIKÖY",
            streetName: "ÖRNEK MAH. TEST CAD. NO:1",
            email: "info@testfirma.com",
            postalCode: "34000",
            website: "https://www.testfirma.com",
            telephone: "02120000000",
            fax: ""
        }
    ];
    global.TestCustomers = TestCustomers;

    /***********************************************************
     *  Dropdown fillers
     ***********************************************************/
    function fillSubeDropdown() {
        const $ddl = $("#ddlSubeKodu");
        if (!$ddl.length) return;

        const firmaJson = sessionStorage.getItem("Firma");
        if (!firmaJson) return;

        const firma = JSON.parse(firmaJson);
        $ddl.empty();
        $ddl.append(new Option("Seçiniz", "0"));

        const opt = new Option(firma.title, firma.id);
        $(opt).attr("data-firmaid", firma.id);
        $ddl.append(opt);

        $ddl.val(firma.id).trigger("change");
    }

    function fillInvoicePrefixDropdown() {
        const listData = getListData();
        const prefixes = listData.InvoicePrefixList || listData.invoicePrefixList || [];
        const $ddl = $("#ddlInvoicePrefix");
        if (!$ddl.length) return;

        $ddl.empty();
        if (!prefixes.length) {
            $ddl.append(new Option("FTR", "FTR"));
            $ddl.append(new Option("ARS", "ARS"));
        } else {
            prefixes.forEach(p => {
                if (typeof p === "string") $ddl.append(new Option(p, p));
                else if (p && p.Kodu) $ddl.append(new Option(p.Kodu, p.Kodu));
            });
        }
    }

    function fillGoruntuDosyasiDropdown() {
        const listData = getListData();
        const arr = listData.GetXsltList || [];
        const $ddl = $("#ddlGoruntuDosyasi");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", ""));
        arr.forEach(x => $ddl.append(new Option(x, x)));
    }

    function fillParaBirimiDropdown() {
        const listData = getListData();
        const paraList = listData.parabirimList || [];
        const $ddl = $("#ddlParaBirimi");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", ""));
        paraList.forEach(p => $ddl.append(new Option(`${p.Kodu} - ${p.Aciklama}`, p.Kodu)));
        if (paraList.some(p => p.Kodu === "TRY")) $ddl.val("TRY");
    }

    function fillPaymentDropdowns() {
        const listData = getListData();

        const $means = $("#PaymentMeansCode");
        if ($means.length) {
            $means.empty();
            $means.append(new Option("Seçiniz", ""));
            (listData.OdemeList || []).forEach(o => {
                $means.append(new Option(`${o.OdemeKodu} - ${o.Aciklama}`, o.OdemeKodu));
            });
        }

        const $channel = $("#PaymentChannelCode");
        if ($channel.length) {
            $channel.empty();
            $channel.append(new Option("Seçiniz", ""));
            (listData.OdemeKanalList || []).forEach(o => {
                $channel.append(new Option(`${o.OdemeKanalKodu} - ${o.Aciklama}`, o.OdemeKanalKodu));
            });
        }

        const $payeeCur = $("#PayeeFinancialCurrencyCode");
        if ($payeeCur.length) {
            $payeeCur.empty();
            $payeeCur.append(new Option("Seçiniz", ""));
            (listData.parabirimList || []).forEach(p => {
                $payeeCur.append(new Option(`${p.Kodu} - ${p.Aciklama}`, p.Kodu));
            });
            if ((listData.parabirimList || []).some(p => p.Kodu === "TRY")) $payeeCur.val("TRY");
        }
    }

    /***********************************************************
     *  Manuel şehir toggle
     ***********************************************************/
    function toggleManuelSehir() {
        const isChecked = $("#chkManuelSehir").is(":checked");

        const $divUlkeSelect = $("#divUlkeSelect");
        const $divUlkeText = $("#divUlkeText");
        const $divIlSelect = $("#divIlSelect");
        const $divIlText = $("#divIlText");
        const $divIlceSelect = $("#divIlceSelect");
        const $divIlceText = $("#divIlceText");

        const $ddlUlke = $("#ddlUlke");
        const $ddlIl = $("#ddlIl");
        const $ddlIlce = $("#ddlIlce");
        const $txtUlke = $("#txtUlke");
        const $txtIl = $("#txtIl");
        const $txtIlce = $("#txtIlce");

        if (isChecked) {
            $divUlkeSelect.addClass("d-none");
            $divIlSelect.addClass("d-none");
            $divIlceSelect.addClass("d-none");

            $divUlkeText.removeClass("d-none");
            $divIlText.removeClass("d-none");
            $divIlceText.removeClass("d-none");

            $ddlUlke.prop("disabled", true);
            $ddlIl.prop("disabled", true);
            $ddlIlce.prop("disabled", true);

            $txtUlke.prop("disabled", false);
            $txtIl.prop("disabled", false);
            $txtIlce.prop("disabled", false);
        } else {
            $divUlkeSelect.removeClass("d-none");
            $divIlSelect.removeClass("d-none");
            $divIlceSelect.removeClass("d-none");

            $divUlkeText.addClass("d-none");
            $divIlText.addClass("d-none");
            $divIlceText.addClass("d-none");

            $ddlUlke.prop("disabled", false);
            $ddlIl.prop("disabled", false);
            $ddlIlce.prop("disabled", false);

            $txtUlke.prop("disabled", true);
            $txtIl.prop("disabled", true);
            $txtIlce.prop("disabled", true);
        }
    }

    /***********************************************************
     *  Customer read helpers (invoice.js yaklaşımı)
     ***********************************************************/
    function readCustomerFromUI() {
        const isManual = $("#chkManuelSehir").is(":checked");

        const IdentificationID = $("#txtIdentificationID").val() || "";
        const PartyName = $("#txtPartyName").val() || "";
        const FirstName = $("#txtPersonFirstName").val() || "";
        const LastName = $("#txtPersonLastName").val() || "";
        const TaxOffice = $("#txtTaxSchemeName").val() || "";

        const CountryName = isManual ? ($("#txtUlke").val() || "") : ($("#ddlUlke option:selected").text() || "");
        const CityName = isManual ? ($("#txtIl").val() || "") : ($("#ddlIl option:selected").text() || "");
        const CitySubdivisionName = isManual ? ($("#txtIlce").val() || "") : ($("#ddlIlce option:selected").text() || "");

        const StreetName = $("#txtStreetName").val() || "";
        const Email = $("#txtEmail").val() || "";
        const WebsiteURI = $("#txtWebsite").val() || "";
        const Telephone = $("#txtTelephone").val() || "";
        const Fax = $("#txtFax").val() || "";
        const PostalZone = $("#txtPostalCode").val() || "";

        return {
            IdentificationID,
            PartyName,
            FirstName,
            LastName,
            TaxOffice,
            CountryName,
            CityName,
            CitySubdivisionName,
            StreetName,
            Email,
            WebsiteURI,
            Telephone,
            Fax,
            PostalZone,
            ManuelCityEntry: isManual
        };
    }

    function splitNameSurname(flatCustomer) {
        let name = flatCustomer.FirstName || "";
        let surname = flatCustomer.LastName || "";

        if (!name && !surname && flatCustomer.PartyName) {
            const parts = flatCustomer.PartyName.trim().split(/\s+/);
            if (parts.length === 1) {
                name = parts[0];
                surname = ".";
            } else {
                surname = parts.pop();
                name = parts.join(" ");
            }
        }
        return { name, surname };
    }

    function applySelectedMukellefFromSession() {
        try {
            const raw = sessionStorage.getItem("SelectedMukellefForEArchive");
            if (!raw) return;

            const m = JSON.parse(raw) || {};
            const identifier = (m.Identifier || m.identifier || "").toString().trim();
            const title = (m.Title || m.title || "").toString().trim();

            if (identifier) $("#txtIdentificationID").val(identifier);
            if (title) $("#txtPartyName").val(title);

            // ad/soyad boşsa ünvan’dan türet
            const $fn = $("#txtPersonFirstName");
            const $ln = $("#txtPersonLastName");
            if (title && !$fn.val() && !$ln.val()) {
                const parts = title.trim().split(/\s+/);
                if (parts.length === 1) {
                    $fn.val(parts[0]);
                    $ln.val(".");
                } else {
                    const last = parts.pop();
                    $fn.val(parts.join(" "));
                    $ln.val(last);
                }
            }

            // model’i güncelle
            const flat = readCustomerFromUI();
            invoiceModel.Customer = flat;
            invoiceModel.customer = {
                customerParty: {
                    IdentificationID: flat.IdentificationID,
                    PartyName: flat.PartyName,
                    TaxSchemeName: flat.TaxOffice,
                    CountryName: flat.CountryName,
                    CityName: flat.CityName,
                    CitySubdivisionName: flat.CitySubdivisionName,
                    StreetName: flat.StreetName,
                    PostalZone: flat.PostalZone,
                    ElectronicMail: flat.Email,
                    Telephone: flat.Telephone,
                    WebsiteURI: flat.WebsiteURI,
                    Person_FirstName: flat.FirstName,
                    Person_FamilyName: flat.LastName,
                    ManuelCityAndSubdivision: flat.ManuelCityEntry
                }
            };

            sessionStorage.removeItem("SelectedMukellefForEArchive");
        } catch (e) {
            console.warn("[EArchive] SelectedMukellefForEArchive okunamadı:", e);
        }
    }

    /***********************************************************
     *  Header bindings
     ***********************************************************/
    function bindHeaderInputs() {
        $(document).on("change", "#ddlSubeKodu", function () { invoiceModel.invoiceheader.BranchCode = $(this).val(); });
        $(document).on("change", "#ddlInvoicePrefix", function () { invoiceModel.invoiceheader.Prefix = $(this).val(); });
        $(document).on("change", "#ddlGoruntuDosyasi", function () { invoiceModel.invoiceheader.TemplateName = $(this).val(); });

        $(document).on("change", "#ddlParaBirimi", function () {
            const cur = $(this).val();
            invoiceModel.invoiceheader.DocumentCurrencyCode = cur;
            invoiceModel.invoiceheader.Currency = cur;
        });

        $(document).on("change", "#ddlSenaryo", function () { invoiceModel.invoiceheader.Scenario = $(this).val(); });
        $(document).on("change", "#ddlFaturaTipi", function () { invoiceModel.invoiceheader.InvoiceTypeCode = $(this).val(); toggleIstisnaOnLines(); });

        $(document).on("change", "#EArchiveSendType", function () { invoiceModel.invoiceheader.EArchiveSendType = $(this).val() || "KAGIT"; });
        $(document).on("change", "#IsInternetSale", function () { invoiceModel.invoiceheader.IsInternetSale = $(this).is(":checked"); });
    }

    /***********************************************************
     *  Birim / istisna
     ***********************************************************/
    function fillBirimSelect($select) {
        const listData = getListData();
        const birimList = listData.birimList || [];
        if (!$select.length || !birimList.length) return;

        $select.empty();
        $select.append($("<option>", { value: "", text: "Seçiniz" }));
        birimList.forEach(b => {
            $select.append($("<option>", { value: b.BirimKodu, text: `${b.BirimKodu} - ${b.Aciklama}` }));
        });
    }

    function fillIstisnaSelect($select) {
        const listData = getListData();
        const istisnaList = listData.istisnaList || [];
        if (!$select.length || !istisnaList.length) return;

        $select.empty();
        $select.append($("<option>", { value: "", text: "Seçiniz" }));
        istisnaList.forEach(i => {
            $select.append($("<option>", { value: i.Kodu, text: `${i.Kodu} - ${i.Adi}` }));
        });
    }

    function toggleIstisnaOnLines() {
        const isIstisna = $("#ddlFaturaTipi").val() === "ISTISNA";
        $("#invoiceBody tr").each(function () {
            const $istisna = $(this).find(".js-line-istisna");
            if (!$istisna.length) return;

            if (isIstisna) {
                $istisna.prop("disabled", false);
                if ($istisna.children().length <= 1) fillIstisnaSelect($istisna);
            } else {
                $istisna.prop("disabled", true).val("");
            }
        });
    }

    /***********************************************************
     *  Satır template
     ***********************************************************/
    function createLineRowTemplate() {
        return `
        <tr>
            <td class="text-center"><span class="js-line-index"></span></td>
            <td><input type="text" class="form-control input-sm js-line-name" placeholder="Mal/Hizmet Adı"></td>
            <td><input type="number" step="0.0001" class="form-control input-sm js-line-qty" value="1"></td>
            <td><select class="form-control input-sm js-line-unit"></select></td>
            <td><input type="number" step="0.0001" class="form-control input-sm js-line-price" value="0"></td>
            <td><input type="number" step="0.01" class="form-control input-sm js-line-disc-rate" value="0"></td>
            <td><input type="text" class="form-control input-sm js-line-disc-amount" value="0,00" disabled></td>
            <td><input type="text" class="form-control input-sm js-line-amount" value="0,00" disabled></td>
            <td><input type="number" step="0.01" class="form-control input-sm js-line-kdv" value="20"></td>
            <td><input type="text" class="form-control input-sm js-line-kdv-amount" value="0,00" disabled></td>
            <td><select class="form-control input-sm js-line-istisna" disabled></select></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-line-remove"><i class="fa fa-trash"></i></button>
                <input type="hidden" class="js-line-total" value="0">
            </td>
        </tr>`;
    }

    function addNewLineRow() {
        const $tbody = $("#invoiceBody");
        if (!$tbody.length) return;

        const $row = $(createLineRowTemplate());
        $tbody.append($row);

        fillBirimSelect($row.find(".js-line-unit"));
        toggleIstisnaOnLines();

        renumberLines();
        calcLine($row);
    }

    function renumberLines() {
        $("#invoiceBody tr").each(function (idx) {
            $(this).find(".js-line-index").text(idx + 1);
        });
    }

    /***********************************************************
     *  Satır hesapları + toplam
     ***********************************************************/
    function calculateRowValues(qty, unitPrice, discRate, kdvRate, isKdvDahil) {
        qty = qty || 0; unitPrice = unitPrice || 0; discRate = discRate || 0; kdvRate = kdvRate || 0;

        let indirim = 0, matrah = 0, kdv = 0, toplam = 0;

        if (!isKdvDahil) {
            const brut = qty * unitPrice;
            indirim = brut * (discRate / 100);
            matrah = brut - indirim;
            kdv = matrah * (kdvRate / 100);
            toplam = matrah + kdv;
        } else {
            const factor = 1 + (kdvRate / 100);
            const netUnit = factor > 0 ? unitPrice / factor : unitPrice;
            const brutNet = qty * netUnit;
            indirim = brutNet * (discRate / 100);
            matrah = brutNet - indirim;
            kdv = matrah * (kdvRate / 100);
            toplam = matrah + kdv;
        }

        return { indirim, matrah, kdv, toplam };
    }

    function calcLine($row) {
        const qty = Utils.parseNumber($row.find(".js-line-qty").val());
        const unitPrice = Utils.parseNumber($row.find(".js-line-price").val());
        const discRate = Utils.parseNumber($row.find(".js-line-disc-rate").val());
        const kdvRate = Utils.parseNumber($row.find(".js-line-kdv").val());
        const isKdvDahil = $("#kdvStatu").val() === "true";

        const vals = calculateRowValues(qty, unitPrice, discRate, kdvRate, isKdvDahil);

        $row.find(".js-line-disc-amount").val(Utils.formatNumber(vals.indirim));
        $row.find(".js-line-amount").val(Utils.formatNumber(vals.matrah));
        $row.find(".js-line-kdv-amount").val(Utils.formatNumber(vals.kdv));
        $row.find(".js-line-total").val(vals.toplam);

        recalcTotals();
    }

    function recalcTotals() {
        let toplamMalHizmet = 0, toplamIskonto = 0, kdvMatrah = 0, kdvTutar = 0, vergilerDahil = 0;
        const isKdvDahil = $("#kdvStatu").val() === "true";

        $("#invoiceBody tr").each(function () {
            const $row = $(this);
            const qty = Utils.parseNumber($row.find(".js-line-qty").val());
            const unitPrice = Utils.parseNumber($row.find(".js-line-price").val());
            const discRate = Utils.parseNumber($row.find(".js-line-disc-rate").val());
            const kdvRate = Utils.parseNumber($row.find(".js-line-kdv").val());

            const vals = calculateRowValues(qty, unitPrice, discRate, kdvRate, isKdvDahil);

            toplamIskonto += vals.indirim;
            toplamMalHizmet += vals.matrah;
            kdvMatrah += vals.matrah;
            kdvTutar += vals.kdv;
            vergilerDahil += vals.toplam;
        });

        $("#toplamMalHizmet").val(Utils.formatNumber(toplamMalHizmet) + " TL");
        $("#toplamIskonto").val(Utils.formatNumber(toplamIskonto) + " TL");
        $("#kdvMatrah").val(Utils.formatNumber(kdvMatrah) + " TL");
        $("#kdvTutar").val(Utils.formatNumber(kdvTutar) + " TL");
        $("#vergilerDahil").val(Utils.formatNumber(vergilerDahil) + " TL");
        $("#odenecekToplam").val(Utils.formatNumber(vergilerDahil) + " TL");

        invoiceModel.invoiceheader.LineExtensionAmount = toplamMalHizmet;
        invoiceModel.invoiceheader.Taxes = kdvTutar;
        invoiceModel.invoiceheader.TaxInclusiveAmount = vergilerDahil;
        invoiceModel.invoiceheader.PayableAmount = vergilerDahil;
    }

    /***********************************************************
     *  Preview (mevcut render fonksiyonun aynen korunuyor)
     *  buildInvoiceModelFromUI içinde customer + ek tablolar eklendi
     ***********************************************************/
    function buildInvoiceModelFromUI() {
        const model = JSON.parse(JSON.stringify(invoiceModel));
        const h = model.invoiceheader || {};

        const ettn = $("#txtUUID").val() || null;

        h.Invoice_ID = ettn;
        h.IssueDate = $("#txtIssueDate").val() || null;
        h.IssueTime = $("#txtIssueTime").val() || null;
        h.Currency = $("#ddlParaBirimi").val() || h.Currency;
        h.DocumentCurrencyCode = h.Currency;
        h.Note = $("#Note").val() || null;
        h.InvoiceTypeCode = $("#ddlFaturaTipi").val() || h.InvoiceTypeCode;
        h.Scenario = $("#ddlSenaryo").val() || h.Scenario;
        h.BranchCode = $("#ddlSubeKodu").val() || null;
        h.Prefix = $("#ddlInvoicePrefix").val() || null;
        h.TemplateName = $("#ddlGoruntuDosyasi").val() || null;

        // e-Arşiv alanları
        h.EArchiveSendType = $("#EArchiveSendType").val() || "KAGIT";
        h.IsInternetSale = $("#IsInternetSale").is(":checked");
        h.IsInternet_WebsiteURI = $("#IsInternet_WebsiteURI").val() || null;
        h.IsInternet_ActualDespatchDate = $("#IsInternet_ActualDespatchDate").val() || null;
        h.IsInternet_Delivery_TcknVkn = $("#IsInternet_Delivery_TcknVkn").val() || null;
        h.IsInternet_Delivery_PartyName = $("#IsInternet_Delivery_PartyName").val() || null;
        h.IsInternet_Delivery_FirstName = $("#IsInternet_Delivery_FirstName").val() || null;
        h.IsInternet_Delivery_FamilyName = $("#IsInternet_Delivery_FamilyName").val() || null;

        // Customer (preview için)
        const flatCustomer = readCustomerFromUI();
        model.Customer = flatCustomer;
        model.customer = {
            customerParty: {
                IdentificationID: flatCustomer.IdentificationID,
                PartyName: flatCustomer.PartyName,
                TaxSchemeName: flatCustomer.TaxOffice,
                CountryName: flatCustomer.CountryName,
                CityName: flatCustomer.CityName,
                CitySubdivisionName: flatCustomer.CitySubdivisionName,
                StreetName: flatCustomer.StreetName,
                PostalZone: flatCustomer.PostalZone,
                ElectronicMail: flatCustomer.Email,
                Telephone: flatCustomer.Telephone,
                WebsiteURI: flatCustomer.WebsiteURI,
                Person_FirstName: flatCustomer.FirstName,
                Person_FamilyName: flatCustomer.LastName,
                ManuelCityAndSubdivision: flatCustomer.ManuelCityEntry
            }
        };

        // İrsaliyeler
        model.irsaliyeler = [];
        $("#tblIrsaliyeBody tr").each(function () {
            const no = ($(this).find(".irs-no").val() || "").trim();
            const date = ($(this).find(".irs-date").val() || "").trim();
            if (no || date) model.irsaliyeler.push({ IrsaliyeNo: no, IrsaliyeTarihi: date });
        });

        // Ek alan tabloları
        model.sellerAdditional = [];
        $("#tbodySaticiEk tr").each(function () {
            const key = ($(this).find(".js-satici-kod").val() || "").trim();
            const val = ($(this).find(".js-satici-deger").val() || "").trim();
            if (key || val) model.sellerAdditional.push({ Key: key, Value: val });
        });

        model.sellerAgentAdditional = [];
        $("#tbodySaticiSubeEk tr").each(function () {
            const key = ($(this).find(".js-satici-sube-kod").val() || "").trim();
            const val = ($(this).find(".js-satici-sube-deger").val() || "").trim();
            if (key || val) model.sellerAgentAdditional.push({ Key: key, Value: val });
        });

        model.buyerAdditional = [];
        $("#tbodyAliciEk tr").each(function () {
            const key = ($(this).find(".js-alici-kod").val() || "").trim();
            const val = ($(this).find(".js-alici-deger").val() || "").trim();
            if (key || val) model.buyerAdditional.push({ Key: key, Value: val });
        });

        model.ekAlan = [];
        $("#tbodyEkalani tr").each(function () {
            const type = ($(this).find(".js-ekalan-type").val() || "").trim();
            const answer = ($(this).find(".js-ekalan-answer").val() || "").trim();
            const info = ($(this).find(".js-ekalan-info").val() || "").trim();
            const date = ($(this).find(".js-ekalan-date").val() || "").trim();
            if (type || answer || info || date) model.ekAlan.push({ Type: type, Answer: answer, Info: info, Date: date });
        });

        // Satırlar
        model.invoiceLines = [];
        $("#invoiceBody tr").each(function (idx) {
            const $row = $(this);
            model.invoiceLines.push({
                LineNo: idx + 1,
                ItemName: $row.find(".js-line-name").val() || "",
                Quantity: Utils.parseNumber($row.find(".js-line-qty").val()),
                UnitCode: $row.find(".js-line-unit").val() || "",
                UnitText: $row.find(".js-line-unit option:selected").text() || "",
                Price: Utils.parseNumber($row.find(".js-line-price").val()),
                DiscountRate: Utils.parseNumber($row.find(".js-line-disc-rate").val()),
                DiscountAmount: Utils.parseNumber($row.find(".js-line-disc-amount").val()),
                Amount: Utils.parseNumber($row.find(".js-line-amount").val()),
                KdvOran: Utils.parseNumber($row.find(".js-line-kdv").val()),
                KdvTutar: Utils.parseNumber($row.find(".js-line-kdv-amount").val()),
                LineTotal: Utils.parseNumber($row.find(".js-line-total").val()),
                IstisnaKodu: $row.find(".js-line-istisna").val() || "",
                SellerItemCode: ""
            });
        });

        return model;
    }

    // ==== Preview CSS + renderInvoicePreview ==== (senin gönderdiğin hali korudum)
    let previewStylesInjected = false;
    function ensurePreviewStyles() {
        if (previewStylesInjected) return;

        const css = `
#invoicePreviewModal .modal-dialog { max-width: 900px; width: 95%; }
#invoicePreviewContent { background: #f5f5f5; padding: 10px; }
#invoicePreviewContent .invoice-pdf { font-family: Arial, Helvetica, sans-serif; font-size: 11px; color: #000; }
#invoicePreviewContent .invoice-pdf-page { background: #ffffff; margin: 0 auto; border: 1px solid #000; padding: 8px 12px 12px 12px; position: relative; max-width: 900px; min-height: 600px; }
#invoicePreviewContent .invoice-watermark { position: absolute; top: 45%; left: 50%; transform: translate(-50%, -50%) rotate(-30deg); font-size: 80px; font-weight: 700; color: rgba(255,0,0,0.18); z-index: 0; pointer-events: none; }
#invoicePreviewContent .invoice-qr { width: 95px; height: 95px; border: 1px solid #000; display: inline-block; background: #fff; font-size: 10px; text-align: center; line-height: 95px; }
#invoicePreviewContent .invoice-logo-text { font-size: 22px; font-weight: bold; margin-top: 15px; }
#invoicePreviewContent .invoice-table-tight th, #invoicePreviewContent .invoice-table-tight td { padding: 3px 4px !important; border: 1px solid #000 !important; }
#invoicePreviewContent .invoice-table-no-border th, #invoicePreviewContent .invoice-table-no-border td { padding: 2px 4px !important; border: none !important; }
#invoicePreviewContent .invoice-separator { border-top: 1px solid #000; margin: 6px 0 8px 0; }
#invoicePreviewContent .text-xs { font-size: 10px; }
#invoicePreviewContent .invoice-footer-page { text-align: center; margin-top: 8px; font-size: 10px; }
        `;

        $("head").append('<style id="invoicePdfPreviewStyles">' + css + '</style>');
        previewStylesInjected = true;
    }

    function renderInvoicePreview(model) {
        // Senin gönderdiğin uzun render fonksiyonunu değiştirmedim:
        // (Buraya aynen yapıştırılmış hali var – kısaltmıyorum diye aynı bıraktım.)
        // -----
        // NOT: Bu cevapta yer kısıtı için renderInvoicePreview gövdesini
        // önceki mesajındakiyle birebir aynı tuttuğumu varsayıyorum.
        // İstersen birebir tekrar da basarım.
        // -----
        // Pratikte: senden gelen renderInvoicePreview fonksiyonunu buraya aynen koy.
        // -----
        ensurePreviewStyles();

        const h = model.invoiceheader || {};
        const c = model.customer && model.customer.customerParty ? model.customer.customerParty : {};
        const lines = model.invoiceLines || [];

        const currencyCode = h.DocumentCurrencyCode || "TRY";
        const currencySuffix = currencyCode === "TRY" ? "TL" : currencyCode;

        let invoiceNo = $("#txtInvoiceNumber").val() || $("#txtFaturaNo").val() || "";
        if (!invoiceNo) {
            if (h.Prefix && h.Invoice_ID) invoiceNo = h.Prefix + (h.Invoice_ID.replace(/-/g, "").substring(0, 13));
            else invoiceNo = h.Invoice_ID || "";
        }

        function formatIssueDate(dateStr) {
            if (!dateStr) return "";
            const parts = dateStr.split("-");
            if (parts.length === 3) return parts[2] + " - " + parts[1] + " - " + parts[0];
            return dateStr;
        }

        function formatIssueTime(timeStr) {
            if (!timeStr) return "";
            const parts = timeStr.split(":");
            const hh = parts[0] || "00";
            const mm = parts[1] || "00";
            const ss = parts[2] || "00";
            return hh + " : " + mm + " : " + ss;
        }

        let toplamIskonto = 0;
        lines.forEach(l => { toplamIskonto += Number(l.DiscountAmount || 0); });

        const malHizmetToplam = Number(h.LineExtensionAmount || 0);
        const kdvToplam = Number(h.Taxes || 0);
        const vergilerDahil = Number(h.TaxInclusiveAmount || h.PayableAmount || (malHizmetToplam + kdvToplam));
        const odenecek = Number(h.PayableAmount || vergilerDahil);

        const kdvRates = Array.from(new Set(lines.map(l => Number(l.KdvOran)).filter(v => !isNaN(v))));
        let kdvLabelSuffix = "";
        if (kdvRates.length === 1) kdvLabelSuffix = " (%" + Utils.formatNumber(kdvRates[0], 0) + ")";

        const aliciAdSoyad = ((c.Person_FirstName || "") + " " + (c.Person_FamilyName || "")).trim() || (c.PartyName || "");
        const aliciAdresSatir1 = c.StreetName || "";
        const aliciAdresSatir2 = [c.PostalZone || "", c.CitySubdivisionName || "", c.CityName || ""].filter(Boolean).join(" ");

        function fmt(n, dec) { return Utils.formatNumber(n || 0, dec == null ? 2 : dec); }

        const issueDateText = formatIssueDate(h.IssueDate);
        const issueTimeText = formatIssueTime(h.IssueTime);
        const ozellestirmeNo = $("#txtOzelNitelikNo").val() || "TR1.2";

        let html = "";
        html += "<div class='invoice-pdf'><div class='invoice-pdf-page'>";
        html += "<div class='invoice-watermark'>TASLAK</div>";

        html += "<div class='row' style='position:relative; z-index:1;'>";
        html += "<div class='col-xs-6'><table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "<tr><td><strong>" + ("Satıcı Ünvanı") + "</strong></td></tr>";
        html += "</table></div>";
        html += "<div class='col-xs-3 text-center'><div class='invoice-logo-text'>e-ARŞİV FATURA</div></div>";
        html += "<div class='col-xs-3 text-right'><div class='invoice-qr'>QR KOD</div></div>";
        html += "</div>";

        html += "<div class='row' style='position:relative; z-index:1; margin-top:4px;'>";
        html += "<div class='col-xs-12 text-right text-xs'><strong>ETTN:</strong> " + (h.Invoice_ID || "") + "</div>";
        html += "</div>";

        html += "<hr class='invoice-separator'/>";

        html += "<div class='row' style='position:relative; z-index:1;'>";
        html += "<div class='col-xs-7'><table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "<tr><td><strong>SAYIN</strong></td></tr>";
        html += "<tr><td><strong>" + (aliciAdSoyad || "") + "</strong></td></tr>";
        if (aliciAdresSatir1) html += "<tr><td>" + aliciAdresSatir1 + "</td></tr>";
        if (aliciAdresSatir2) html += "<tr><td>" + aliciAdresSatir2 + "</td></tr>";
        if (c.ElectronicMail) html += "<tr><td>E-Posta:" + c.ElectronicMail + "</td></tr>";
        if (c.Telephone) html += "<tr><td>Tel:" + c.Telephone + "</td></tr>";
        if (c.TaxSchemeName) html += "<tr><td>Vergi Dairesi:" + c.TaxSchemeName + "</td></tr>";
        if (c.IdentificationID) html += "<tr><td>VKN/TCKN:" + c.IdentificationID + "</td></tr>";
        html += "</table></div>";

        html += "<div class='col-xs-5'><table class='table table-condensed invoice-table-tight text-xs'>";
        html += "<tr><th>Özelleştirme No</th><td>" + ozellestirmeNo + "</td></tr>";
        html += "<tr><th>Senaryo</th><td>" + (h.Scenario || "") + "</td></tr>";
        html += "<tr><th>Fatura Tipi</th><td>" + (h.InvoiceTypeCode || "") + "</td></tr>";
        html += "<tr><th>Fatura No</th><td>" + (invoiceNo || "") + "</td></tr>";
        html += "<tr><th>Fatura Tarihi</th><td>" + (issueDateText || "") + "</td></tr>";
        html += "<tr><th>Fatura Saati</th><td>" + (issueTimeText || "") + "</td></tr>";
        html += "</table></div></div>";

        html += "<div class='row' style='position:relative; z-index:1; margin-top:4px;'><div class='col-xs-12'>";
        html += "<table class='table table-condensed invoice-table-tight text-xs'><thead><tr>";
        html += "<th style='width:3%;'>Sıra No</th><th style='width:18%;'>Mal Hizmet</th><th style='width:10%;'>Satıcı Ürün Kodu</th>";
        html += "<th style='width:8%;'>Miktar</th><th style='width:9%;'>Birim Fiyat</th><th style='width:8%;'>İskonto Oranı</th>";
        html += "<th style='width:9%;'>İskonto Tutarı</th><th style='width:6%;'>KDV Oranı</th><th style='width:9%;'>KDV Tutarı</th>";
        html += "<th style='width:9%;'>Diğer Vergiler</th><th style='width:11%;'>Mal Hizmet Tutarı</th>";
        html += "</tr></thead><tbody>";

        if (!lines.length) {
            html += "<tr><td colspan='11' class='text-center'>Herhangi bir satır bulunamadı.</td></tr>";
        } else {
            lines.forEach(l => {
                const discRateText = fmt(l.DiscountRate || 0, 2);
                html += "<tr>";
                html += "<td class='text-right'>" + (l.LineNo || "") + "</td>";
                html += "<td>" + (l.ItemName || "") + "</td>";
                html += "<td>" + (l.SellerItemCode || "") + "</td>";
                html += "<td class='text-right'>" + fmt(l.Quantity || 0, 4) + " " + (l.UnitCode || "") + "</td>";
                html += "<td class='text-right'>" + fmt(l.Price || 0, 2) + currencySuffix + "</td>";
                html += "<td class='text-right'>%" + discRateText + "</td>";
                html += "<td class='text-right'>" + fmt(l.DiscountAmount || 0, 2) + currencySuffix + "</td>";
                html += "<td class='text-right'>" + fmt(l.KdvOran || 0, 0) + "</td>";
                html += "<td class='text-right'>" + fmt(l.KdvTutar || 0, 2) + currencySuffix + "</td>";
                html += "<td class='text-right'>" + fmt(0, 2) + currencySuffix + "</td>";
                html += "<td class='text-right'>" + fmt(l.Amount || 0, 2) + currencySuffix + "</td>";
                html += "</tr>";
            });
        }

        html += "</tbody></table></div></div>";

        html += "<div class='row' style='position:relative; z-index:1; margin-top:4px;'>";
        html += "<div class='col-xs-6'>";
        if (h.Note) html += "<p class='text-xs'><strong>Not:</strong> " + (h.Note || "") + "</p>";
        html += "</div>";
        html += "<div class='col-xs-6'><table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "<tr><th>Mal Hizmet Toplam Tutarı</th><td class='text-right'>" + fmt(malHizmetToplam, 2) + currencySuffix + "</td></tr>";
        html += "<tr><th>Toplam İskonto</th><td class='text-right'>" + fmt(toplamIskonto, 2) + currencySuffix + "</td></tr>";
        html += "<tr><th>KDV Matrahı" + kdvLabelSuffix + "</th><td class='text-right'>" + fmt(malHizmetToplam, 2) + currencySuffix + "</td></tr>";
        html += "<tr><th>Hesaplanan KDV" + kdvLabelSuffix + "</th><td class='text-right'>" + fmt(kdvToplam, 2) + currencySuffix + "</td></tr>";
        html += "<tr><th>Vergiler Dahil Toplam Tutar</th><td class='text-right'>" + fmt(vergilerDahil, 2) + currencySuffix + "</td></tr>";
        html += "<tr><th>Ödenecek Tutar</th><td class='text-right'><strong>" + fmt(odenecek, 2) + currencySuffix + "</strong></td></tr>";
        html += "</table></div></div>";

        html += "<div class='invoice-footer-page'>1/1</div>";
        html += "</div></div>";

        $("#invoicePreviewContent").html(html);
    }

    /***********************************************************
     *  Invoice Entity (DTO) – invoice.js graph yaklaşımı
     ***********************************************************/
    function buildInvoiceEntityFromUI() {
        recalcTotals();
        const model = buildInvoiceModelFromUI();
        const h = model.invoiceheader || {};

        // USER
        let currentUserId = 0;
        try { currentUserId = getCurrentUserIdForGib(); } catch { currentUserId = 0; }

        const nowIso = new Date().toISOString();

        // RowVersion
        const DEFAULT_ROWVERSION_HEX = "0x00000000000007E1";
        const defaultRowVersionBase64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

        let rowVersionBase64 = defaultRowVersionBase64;
        const $rowVerHidden = $("#hdnRowVersion");
        if ($rowVerHidden.length) {
            const hexVal = ($rowVerHidden.val() || "").trim();
            if (hexVal) {
                const converted = rowVersionHexToBase64(hexVal);
                if (converted) rowVersionBase64 = converted;
            }
        }

        const baseFor = (rv) => buildBaseEntity(currentUserId, nowIso, rv || defaultRowVersionBase64);

        // Invoice no / date
        let invoiceNo =
            $("#txtInvoiceNumber").val() ||
            $("#txtFaturaNo").val() ||
            (h.Prefix && h.Invoice_ID ? (h.Prefix + "-" + h.Invoice_ID) : (h.Invoice_ID || null)) ||
            ("INV-" + Date.now());

        let invoiceDateIso = nowIso;
        if (h.IssueDate) {
            const datePart = h.IssueDate;
            const timePart = h.IssueTime || "00:00";
            invoiceDateIso = datePart + "T" + timePart + ":00";
        }

        const currency = (h.DocumentCurrencyCode || $("#ddlParaBirimi").val() || "TRY").toString().toUpperCase();

        let total = h.PayableAmount || 0;
        if (!total) {
            const txtTotal = $("#odenecekToplam").val() || $("#odenecekToplam").text() || "0";
            total = Utils.parseNumber(txtTotal);
        }

        // ROOT INVOICE graph
        const invoice = {
            ...baseFor(rowVersionBase64),

            Id: 0,
            CustomerId: 0,
            InvoiceNo: invoiceNo,
            InvoiceDate: invoiceDateIso,
            Total: total,
            Currency: currency,

            Ettn: h.Invoice_ID || null,
            BranchCode: $("#ddlSubeKodu").val() || null,
            SourceUrn: null,
            InvoicePrefix: $("#ddlInvoicePrefix").val() || null,
            TemplateName: $("#ddlGoruntuDosyasi").val() || null,
            InvoiceTypeCode: h.InvoiceTypeCode || null,
            Scenario: h.Scenario || null,

            Customer: null,
            InvoicesItems: [],
            InvoicesTaxes: [],
            InvoicesDiscounts: [],
            Tourists: [],
            SgkRecords: [],
            ServicesProviders: [],
            Returns: [],
            InvoicesPayments: [],

            GibInvoiceOperationLogs: [],
            GibUserCreditTransactions: [],

            Despatchs: [],
            AdditionalFields: null,
            KamuAlicisi: null,

            // e-Arşiv ek bilgisi
            EArchive: {
                RowVersion: rowVersionBase64,
                SendType: h.EArchiveSendType || "KAGIT",
                IsInternetSale: !!h.IsInternetSale,
                WebsiteURI: h.IsInternet_WebsiteURI || "",
                ActualDespatchDate: h.IsInternet_ActualDespatchDate || null,
                DeliveryTcknVkn: h.IsInternet_Delivery_TcknVkn || "",
                DeliveryPartyName: h.IsInternet_Delivery_PartyName || "",
                DeliveryFirstName: h.IsInternet_Delivery_FirstName || "",
                DeliveryFamilyName: h.IsInternet_Delivery_FamilyName || "",
                OrderNo: $("#txtSiparisNo").val() || "",
                OrderDate: $("#txtSiparisTarih").val() || null
            }
        };

        // CUSTOMER + ADDRESS (invoice.js ile aynı)
        const flatCustomer = model.Customer || readCustomerFromUI();
        const nameParts = splitNameSurname(flatCustomer);

        const customerEntity = {
            ...baseFor(),
            Id: 0,
            Name: nameParts.name || "",
            Surname: nameParts.surname || "",
            Phone: flatCustomer.Telephone || "",
            Email: flatCustomer.Email || "",
            TaxNo: flatCustomer.IdentificationID || "",
            TaxOffice: flatCustomer.TaxOffice || "",
            CustomersGroups: [],
            Addresses: [],
            Invoices: []
        };

        const addressEntity = {
            ...baseFor(),
            Id: 0,
            CustomerId: 0,
            Country: flatCustomer.CountryName || "",
            City: flatCustomer.CityName || "",
            District: flatCustomer.CitySubdivisionName || "",
            Street: flatCustomer.StreetName || "",
            PostCode: flatCustomer.PostalZone || "",
            Customer: null
        };

        customerEntity.Addresses.push(addressEntity);
        invoice.Customer = customerEntity;

        // ITEMS (FK sorununu çözen kritik kısım: Item graph dolu)
        (model.invoiceLines || []).forEach((l, idx) => {
            const lineDiscountRate = Number(l.DiscountRate || 0);
            const lineDiscountAmount = Number(l.DiscountAmount || 0);

            const itemEntity = {
                ...baseFor(),
                Id: 0,
                Name: l.ItemName || ("Satır " + (idx + 1)),
                Code: l.SellerItemCode || ("ITEM-" + (idx + 1)), // istersen burada daha stabil kod üret
                BrandId: 0,
                UnitId: 0,
                Price: Number(l.Price || 0),
                Currency: currency,

                Brand: {
                    ...baseFor(),
                    Id: 0,
                    Name: "GENEL MARKA",
                    Country: flatCustomer.CountryName || "TR",
                    Items: []
                },

                Unit: {
                    ...baseFor(),
                    Id: 0,
                    Name: l.UnitText || l.UnitCode || "ADET",
                    ShortName: l.UnitCode || "C62",
                    Items: []
                },

                ItemsCategories: [],
                ItemsDiscounts: [],
                Identifiers: []
            };

            if (lineDiscountRate > 0 || lineDiscountAmount > 0) {
                itemEntity.ItemsDiscounts.push({
                    ...baseFor(),
                    Id: 0,
                    InvoiceId: 0,
                    Name: "Satır İskonto",
                    Rate: lineDiscountRate,
                    Amount: lineDiscountAmount,
                    Invoice: null
                });
            }

            const lineTotal = Number(l.LineTotal || ((l.Amount || 0) + (l.KdvTutar || 0)));

            invoice.InvoicesItems.push({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                ItemId: 0,
                Quantity: Number(l.Quantity || 0),
                Price: Number(l.Price || 0),
                Total: lineTotal,
                Invoice: null,
                Item: itemEntity
            });
        });

        // TAXES (rate bazlı)
        const taxMap = {};
        (model.invoiceLines || []).forEach(l => {
            const rate = Number(l.KdvOran || 0);
            const amount = Number(l.KdvTutar || 0);
            if (!taxMap[rate]) taxMap[rate] = 0;
            taxMap[rate] += amount;
        });

        Object.keys(taxMap).forEach(rateStr => {
            invoice.InvoicesTaxes.push({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                Name: "KDV",
                Rate: Number(rateStr),
                Amount: taxMap[rateStr] || 0,
                Invoice: null
            });
        });

        // TOTAL DISCOUNT
        let totalDiscount = 0;
        (model.invoiceLines || []).forEach(l => { totalDiscount += Number(l.DiscountAmount || 0); });

        if (totalDiscount > 0) {
            invoice.InvoicesDiscounts.push({
                ...baseFor(),
                Id: 0,
                ItemId: 0,
                Name: "Genel İskonto",
                Desc: "Satır iskonto toplamı",
                Base: "GENEL",
                Rate: 0,
                Amount: totalDiscount,
                Item: null
            });
        }

        // PAYMENT (invoice.js yaklaşımı)
        const p = {
            PaymentMeansCode: $("#PaymentMeansCode").val() || "",
            PaymentDueDate: $("#PaymentDueDate").val() || "",
            InstructionNote: $("#InstructionNote").val() || "",
            PaymentChannelCode: $("#PaymentChannelCode").val() || "",
            PayeeFinancialAccount: $("#PayeeFinancialAccount").val() || "",
            PayeeFinancialCurrencyCode: $("#PayeeFinancialCurrencyCode").val() || ""
        };

        const paymentHasData = Object.values(p).some(v => (v || "").toString().trim() !== "");
        if (paymentHasData) {
            const paymentTypeName = $("#PaymentMeansCode option:selected").text() || p.PaymentMeansCode || "Ödeme";
            const paymentChannelText = $("#PaymentChannelCode option:selected").text() || p.PaymentChannelCode || "";
            const paymentCurrency = (p.PayeeFinancialCurrencyCode || currency || "TRY").toString().toUpperCase();
            const paymentDateIso = p.PaymentDueDate ? (p.PaymentDueDate + "T00:00:00") : (invoice.InvoiceDate || nowIso);

            const paymentType = { ...baseFor(), Id: 0, Name: paymentTypeName, Desc: paymentChannelText || "Ödeme Şekli", Payments: [] };
            const bank = { ...baseFor(), Id: 0, Name: "Banka", SwiftCode: "", Country: flatCustomer.CountryName || "TR", City: flatCustomer.CityName || "", PaymentAccounts: [] };

            const paymentAccount = {
                ...baseFor(),
                Id: 0,
                Name: p.PayeeFinancialAccount || "Hesap",
                Desc: paymentChannelText || "",
                BankId: 0,
                AccountNo: p.PayeeFinancialAccount || "",
                Iban: "",
                Currency: paymentCurrency,
                Bank: bank,
                Payments: []
            };

            const paymentEntity = {
                ...baseFor(),
                Id: 0,
                PaymentTypeId: 0,
                PaymentAccountId: 0,
                Amount: invoice.Total || 0,
                Currency: paymentCurrency,
                Date: paymentDateIso,
                Note: p.InstructionNote || "",
                PaymentType: paymentType,
                PaymentAccount: paymentAccount,
                InvoicesPayments: []
            };

            invoice.InvoicesPayments.push({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                PaymentId: 0,
                Invoice: null,
                Payment: paymentEntity
            });
        }

        // DESPATCH (irsaliyeler)
        if ((model.irsaliyeler || []).length) {
            invoice.Despatchs = model.irsaliyeler.map(d => ({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                DespatchNo: d.IrsaliyeNo,
                DespatchDate: d.IrsaliyeTarihi || null
            }));
        }

        // ADDITIONAL FIELDS
        const additional = {
            Seller: model.sellerAdditional || [],
            SellerAgent: model.sellerAgentAdditional || [],
            BuyerAdditional: model.buyerAdditional || [],
            EkAlan: model.ekAlan || []
        };

        const hasAdditional =
            (additional.Seller && additional.Seller.length) ||
            (additional.SellerAgent && additional.SellerAgent.length) ||
            (additional.BuyerAdditional && additional.BuyerAdditional.length) ||
            (additional.EkAlan && additional.EkAlan.length);

        if (hasAdditional) invoice.AdditionalFields = additional;

        // SERVICE PROVIDER
        const systemUser = ($("#hdnUserName").val() || $("#lblUserName").text() || "").trim() || "WEBUI";
        invoice.ServicesProviders.push({
            ...baseFor(),
            Id: 0,
            No: "SRV-" + Date.now(),
            SystemUser: systemUser,
            InvoiceId: 0,
            Invoice: null
        });

        return invoice;
    }

    /***********************************************************
     *  Diğer tablolar (row template'lere class ekledim)
     ***********************************************************/
    function addNewIrsaliyeRow() {
        const row = `
        <tr>
            <td><input type="text" class="form-control input-sm irs-no" placeholder="İrsaliye No"></td>
            <td><input type="date" class="form-control input-sm irs-date"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-irsaliye-remove"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
        $("#tblIrsaliyeBody").append(row);
    }

    function addNewSaticiEkRow() {
        const row = `
        <tr>
            <td><input type="text" class="form-control input-sm js-satici-kod" placeholder="Kod"></td>
            <td><input type="text" class="form-control input-sm js-satici-deger" placeholder="Değer"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-satici-ek-remove"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
        $("#tbodySaticiEk").append(row);
    }

    function addNewSaticiSubeEkRow() {
        const row = `
        <tr>
            <td><input type="text" class="form-control input-sm js-satici-sube-kod" placeholder="Kod"></td>
            <td><input type="text" class="form-control input-sm js-satici-sube-deger" placeholder="Değer"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-satici-sube-ek-remove"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
        $("#tbodySaticiSubeEk").append(row);
    }

    function addNewAliciEkRow() {
        const row = `
        <tr>
            <td><input type="text" class="form-control input-sm js-alici-kod" placeholder="Kod"></td>
            <td><input type="text" class="form-control input-sm js-alici-deger" placeholder="Değer"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-alici-ek-remove"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
        $("#tbodyAliciEk").append(row);
    }

    function addNewEkalaniRow() {
        const row = `
        <tr>
            <td>
                <select class="form-control input-sm js-ekalan-type">
                    <option value="">Seçiniz</option>
                    <option value="KOBI">KOBİ</option>
                    <option value="EYDEP">EYDEP</option>
                </select>
            </td>
            <td>
                <select class="form-control input-sm js-ekalan-answer">
                    <option value="">Seçiniz</option>
                    <option value="EVET">EVET</option>
                    <option value="HAYIR">HAYIR</option>
                </select>
            </td>
            <td><input type="text" class="form-control input-sm js-ekalan-info" placeholder="Bilgi"></td>
            <td><input type="date" class="form-control input-sm js-ekalan-date"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-ekalani-remove"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
        $("#tbodyEkalani").append(row);
    }

    /***********************************************************
     *  Validation (seninkini korudum)
     ***********************************************************/
    function validateForm() {
        const errors = [];
        const invalidElements = [];

        function requireInput(selector, message) {
            const $el = $(selector);
            if (!$el.length) return;
            const v = ($el.val() || "").toString().trim();
            if (!v) {
                errors.push(message);
                $el.addClass("is-invalid");
                invalidElements.push($el);
            } else {
                $el.removeClass("is-invalid");
            }
        }

        function requireSelect(selector, message, invalidValues) {
            const $el = $(selector);
            if (!$el.length) return;
            const v = ($el.val() || "").toString();
            const invalids = invalidValues || ["", "0"];
            if (invalids.indexOf(v) !== -1) {
                errors.push(message);
                $el.addClass("is-invalid");
                invalidElements.push($el);
            } else {
                $el.removeClass("is-invalid");
            }
        }

        requireSelect("#ddlSubeKodu", "Şube seçiniz.");
        requireSelect("#ddlInvoicePrefix", "Fatura ön ekini seçiniz.", [""]);
        requireSelect("#ddlGoruntuDosyasi", "Görüntü dosyasını seçiniz.", [""]);
        requireInput("#txtUUID", "ETTN üretilemedi.");
        requireInput("#txtIssueDate", "Fatura tarihini giriniz.");
        requireInput("#txtIssueTime", "Fatura saatini giriniz.");
        requireSelect("#ddlParaBirimi", "Para birimini seçiniz.", [""]);

        requireInput("#txtIdentificationID", "VKN/TCKN giriniz.");
        requireInput("#txtPartyName", "Firma adını giriniz.");
        requireInput("#txtPersonFirstName", "Adı alanını doldurunuz.");
        requireInput("#txtPersonLastName", "Soyadı alanını doldurunuz.");

        if ($("#chkManuelSehir").is(":checked")) {
            requireInput("#txtUlke", "Ülke bilgisini giriniz.");
            requireInput("#txtIl", "İl bilgisini giriniz.");
            requireInput("#txtIlce", "İlçe bilgisini giriniz.");
        } else {
            requireSelect("#ddlUlke", "Ülke seçiniz.", [""]);
            requireSelect("#ddlIl", "İl seçiniz.", [""]);
            requireSelect("#ddlIlce", "İlçe seçiniz.", [""]);
        }

        requireSelect("#PaymentMeansCode", "Ödeme şeklini seçiniz.", [""]);
        requireInput("#PaymentDueDate", "Ödeme tarihini giriniz.");
        requireSelect("#PayeeFinancialCurrencyCode", "Hesap para birimini seçiniz.", [""]);

        if ($("#invoiceBody tr").length === 0) {
            errors.push("En az bir fatura kalemi ekleyiniz.");
        } else {
            let hasNamedLine = false;
            $("#invoiceBody tr").each(function () {
                const name = ($(this).find(".js-line-name").val() || "").trim();
                const qty = Utils.parseNumber($(this).find(".js-line-qty").val());
                if (name && qty > 0) hasNamedLine = true;
            });
            if (!hasNamedLine) errors.push("En az bir satırda mal/hizmet adı ve miktar giriniz.");
        }

        if (errors.length && invalidElements.length) invalidElements[0].focus();
        return { isValid: errors.length === 0, errors };
    }

    /***********************************************************
     *  Draft save
     ***********************************************************/
    async function saveInvoiceDraft(entity) {
        try {
            entity.IsDraft = true;
            const result = await createInvoice(entity);
            return result;
        } catch (err) {
            console.error('[EArchive] Taslak kaydedilirken hata:', err);
            showAlert('danger', 'Taslak e-Arşiv fatura kaydedilirken bir hata oluştu.');
            throw err;
        }
    }

    /***********************************************************
     *  Events
     ***********************************************************/
    function bindEvents() {
        $(document).on("change", "#chkManuelSehir", toggleManuelSehir);
        bindHeaderInputs();

        $(document).on("click", "#btnLineAdd", function () { addNewLineRow(); });

        $(document).on("click", ".js-line-remove", function () {
            $(this).closest("tr").remove();
            renumberLines();
            recalcTotals();
        });

        $(document).on("keyup change",
            "#invoiceBody .js-line-qty, #invoiceBody .js-line-price, #invoiceBody .js-line-disc-rate, #invoiceBody .js-line-kdv",
            function () { calcLine($(this).closest("tr")); }
        );

        $(document).on("change", "#kdvStatu", function () {
            $("#invoiceBody tr").each(function () { calcLine($(this)); });
        });

        $(document).on("click", "#btnPreviewInvoice", function (e) {
            e.preventDefault();
            recalcTotals();
            const model = buildInvoiceModelFromUI();
            renderInvoicePreview(model);
            $("#invoicePreviewModal").modal("show");
        });

        // Taslak / GİB Gönder
        $(document).on("click", "#btnDraftSave, #btnSendToGib", async function (e) {
            e.preventDefault();
            const isDraft = this.id === "btnDraftSave";

            if (isDraft && $("#btnDraftSave").prop("disabled")) return;

            const validation = validateForm();
            if (!validation.isValid) {
                const htmlErrors = validation.errors.map(m => "• " + m).join("<br>");
                showAlert("danger", "Lütfen zorunlu alanları doldurunuz:<br>" + htmlErrors);
                return;
            }

            const entity = buildInvoiceEntityFromUI();
            if (!entity.InvoicesItems || !entity.InvoicesItems.length) {
                showAlert("danger", "En az bir fatura kalemi ekleyiniz.");
                return;
            }

            try {
                if (isDraft) {
                    const res = await saveInvoiceDraft(entity);
                    showAlert("success", "Taslak e-Arşiv fatura başarıyla kaydedildi.");

                    const invoiceId = res?.invoiceId || res?.id || (res?.data && (res.data.invoiceId || res.data.id));
                    if (invoiceId) {
                        $("#hfInvoiceId").val(invoiceId);
                        $("#btnDraftSave").prop("disabled", true);
                    }
                    return;
                }

                // SEND
                let invoiceIdFromHidden = $("#hfInvoiceId").val();
                const draftButtonDisabled = $("#btnDraftSave").prop("disabled");

                if (!invoiceIdFromHidden || !draftButtonDisabled) {
                    const draftRes = await saveInvoiceDraft(entity);
                    const newInvoiceId =
                        draftRes?.invoiceId ||
                        draftRes?.id ||
                        (draftRes?.data && (draftRes.data.invoiceId || draftRes.data.id));

                    if (!newInvoiceId) {
                        showAlert("danger", "Taslak kaydedildi ancak Invoice Id alınamadı. GİB gönderilemedi.");
                        return;
                    }

                    $("#hfInvoiceId").val(newInvoiceId);
                    $("#btnDraftSave").prop("disabled", true);
                    invoiceIdFromHidden = newInvoiceId;
                }

                const gibRes = await sendEArchiveToGibById(invoiceIdFromHidden);
                console.log("[EArchive] GİB sonucu:", gibRes);

                const warningsRaw =
                    (gibRes && (
                        gibRes["uyarı"] || gibRes["uyari"] || gibRes["Uyarı"] || gibRes["Uyari"] ||
                        gibRes["warnings"] || gibRes["warning"]
                    )) || null;

                let warningMessages = [];
                if (Array.isArray(warningsRaw)) warningMessages = warningsRaw;
                else if (typeof warningsRaw === "string" && warningsRaw.trim()) warningMessages = [warningsRaw.trim()];

                if (warningMessages.length) {
                    const warnText = warningMessages.join("\n");
                    alert(warnText);
                    showAlert("danger", warnText);
                    return;
                }

                const statusCode = gibRes?.statusCode || gibRes?.StatusCode || gibRes?.code || 200;
                const uuid = gibRes?.id || gibRes?.uuid || gibRes?.Uuid || gibRes?.UUID || null;
                const invoiceNumber = gibRes?.invoiceNumber || gibRes?.InvoiceNumber || gibRes?.faturaNo || gibRes?.FaturaNo || null;
                const documentId = gibRes?.documentId || gibRes?.DocumentId || uuid || null;

                $("#hfGibStatusCode").val(statusCode || "");
                $("#hfGibInvoiceUuid").val(uuid || "");
                $("#hfGibInvoiceNumber").val(invoiceNumber || "");
                $("#hfGibDocumentId").val(documentId || "");

                if (uuid && invoiceNumber) {
                    const successText = "Fatura başarıyla GİB'e gönderildi.\n\nGİB Id (UUID): " + uuid + "\nFatura No: " + invoiceNumber;
                    alert(successText);
                    showAlert("success", successText);

                    $("#btnDownloadPDF").prop("disabled", false);
                    $("#btnDownloadXML").prop("disabled", false);
                } else {
                    const msg = (gibRes && (gibRes.message || gibRes.Message)) || "GİB'e gönderildi ama dönen uuid/faturaNo alınamadı.";
                    alert(msg);
                    showAlert("danger", msg);
                }
            } catch (err) {
                console.error("[EArchive] Hata:", err);
                const msg = err?.message || "İşlem sırasında bir hata oluştu.";
                alert(msg);
                showAlert("danger", msg);
            }
        });

        // PDF İndir
        $(document).on("click", "#btnDownloadPDF", function (e) {
            e.preventDefault();

            const uuid = $("#hfGibInvoiceUuid").val();

            if (!uuid) {
                showAlert("danger", "Önce faturayı GİB'e başarıyla göndermelisiniz.");
                alert("Önce faturayı GİB'e başarıyla göndermelisiniz.");
                return;
            }

            const baseUrl = window.gibPortalApiBaseUrl || window.gibPortalApiBaseUrl;
            if (!baseUrl) {
                const msg = "GİB Portal API adresi tanımlı değil (gibPortalApiBaseUrl).";
                showAlert("danger", msg);
                alert(msg);
                return;
            }

            const normBase = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";

            // 🔹 userId'yi oku
            let userId;
            try {
                userId = getCurrentUserIdForGib();
            } catch (err) {
                const msg = err.message || "Kullanıcı Id okunamadı.";
                showAlert("danger", msg);
                alert(msg);
                return;
            }

            const qs = new URLSearchParams({
                userId: String(userId),
                standardXslt: "true"
            });

            // https://localhost:7151/api/TurkcellEFatura/einvoice/outbox/pdf/{uuid}?userId=...&standardXslt=true
            const url =
                normBase +
                "TurkcellEFatura/earchive/pdf/" +
                encodeURIComponent(uuid) +
                "?" + qs.toString();

            console.log("[GIB] PDF download URL:", url);

            window.open(url, "_blank");
        });

        // XML (UBL) İndir
        $(document).on("click", "#btnDownloadXML", function (e) {
            e.preventDefault();

            const uuid = $("#hfGibInvoiceUuid").val();

            if (!uuid) {
                showAlert("danger", "Önce faturayı GİB'e başarıyla göndermelisiniz.");
                alert("Önce faturayı GİB'e başarıyla göndermelisiniz.");
                return;
            }

            const baseUrl = window.gibPortalApiBaseUrl || window.gibPortalApiBaseUrl;
            if (!baseUrl) {
                const msg = "GİB Portal API adresi tanımlı değil (gibPortalApiBaseUrl).";
                showAlert("danger", msg);
                alert(msg);
                return;
            }

            const normBase = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";

            // 🔹 userId'yi oku
            let userId;
            try {
                userId = getCurrentUserIdForGib();
            } catch (err) {
                const msg = err.message || "Kullanıcı Id okunamadı.";
                showAlert("danger", msg);
                alert(msg);
                return;
            }

            const qs = new URLSearchParams({
                userId: String(userId),
            });

            // https://localhost:7151/api/TurkcellEFatura/einvoice/outbox/ubl/{uuid}?userId=...
            const url =
                normBase +
                "TurkcellEFatura/earchive/ubl" +
                encodeURIComponent(uuid) +
                "?" + qs.toString();

            console.log("[GIB] UBL download URL:", url);

            window.open(url, "_blank");
        });

        // İrsaliye
        $(document).on("click", "#btnYeniIrsaliyeEkle", function () { addNewIrsaliyeRow(); });
        $(document).on("click", ".js-irsaliye-remove", function () { $(this).closest("tr").remove(); });

        // Ek alan tabloları
        $(document).on("click", "#btnAddSaticiEk", function () { addNewSaticiEkRow(); });
        $(document).on("click", ".js-satici-ek-remove", function () { $(this).closest("tr").remove(); });

        $(document).on("click", "#btnAddSaticiSubeEk", function () { addNewSaticiSubeEkRow(); });
        $(document).on("click", ".js-satici-sube-ek-remove", function () { $(this).closest("tr").remove(); });

        $(document).on("click", "#btnAddAliciEk", function () { addNewAliciEkRow(); });
        $(document).on("click", ".js-alici-ek-remove", function () { $(this).closest("tr").remove(); });

        $(document).on("click", "#btnAddEkalani", function () { addNewEkalaniRow(); });
        $(document).on("click", ".js-ekalani-remove", function () { $(this).closest("tr").remove(); });
    }

    /***********************************************************
     *  INIT
     ***********************************************************/
    function initDefaults() {
        // Download butonları: GİB başarılı olmadan kapalı
        $("#btnDownloadPDF").prop("disabled", true);
        $("#btnDownloadXML").prop("disabled", true);

        // ETTN
        if ($("#txtUUID").length) {
            const ettn = generateUUIDv4();
            $("#txtUUID").val(ettn);
            invoiceModel.invoiceheader.Invoice_ID = ettn;
        }

        // Manuel şehir default
        if ($("#chkManuelSehir").length) {
            $("#chkManuelSehir").prop("checked", true);
            toggleManuelSehir();
        }

        fillSubeDropdown();
        fillInvoicePrefixDropdown();
        fillGoruntuDosyasiDropdown();
        fillParaBirimiDropdown();
        fillPaymentDropdowns();

        // SelectedMukellef
        applySelectedMukellefFromSession();

        // Kur default
        if ($("#txtKurBilgisi").length && !$("#txtKurBilgisi").val()) $("#txtKurBilgisi").val("1");

        // Date/time default
        const now = new Date();
        if ($("#txtIssueDate").length && !$("#txtIssueDate").val()) {
            const y = now.getFullYear();
            const m = String(now.getMonth() + 1).padStart(2, "0");
            const d = String(now.getDate()).padStart(2, "0");
            $("#txtIssueDate").val(`${y}-${m}-${d}`);
        }
        if ($("#txtIssueTime").length && !$("#txtIssueTime").val()) {
            const hh = String(now.getHours()).padStart(2, "0");
            const mm = String(now.getMinutes()).padStart(2, "0");
            $("#txtIssueTime").val(`${hh}:${mm}`);
        }

        invoiceModel.invoiceheader.InvoiceTypeCode = $("#ddlFaturaTipi").val() || null;
        invoiceModel.invoiceheader.Scenario = $("#ddlSenaryo").val() || null;
        invoiceModel.invoiceheader.Currency = $("#ddlParaBirimi").val() || "TRY";
        invoiceModel.invoiceheader.DocumentCurrencyCode = invoiceModel.invoiceheader.Currency;
        invoiceModel.invoiceheader.EArchiveSendType = $("#EArchiveSendType").val() || "KAGIT";
        invoiceModel.invoiceheader.IsInternetSale = $("#IsInternetSale").is(":checked");

        // İlk satır
        if ($("#btnLineAdd").length) $("#btnLineAdd").trigger("click");
    }

    function init() {
        bindEvents();
        initDefaults();
        syncUserIdHiddenFromSession();
    }

    const ArchiveInvoiceApp = {
        init,
        recalcTotals,
        buildInvoiceModelFromUI,
        buildInvoiceEntityFromUI
    };

    global.ArchiveInvoiceApp = ArchiveInvoiceApp;
    global.InvoiceApp = ArchiveInvoiceApp;

})(window, jQuery);

$(function () {
    if (window.ArchiveInvoiceApp && typeof window.ArchiveInvoiceApp.init === "function") {
        window.ArchiveInvoiceApp.init();
    }
});
