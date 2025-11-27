
// Login.js'teki gibi: Service'den create fonksiyonunu al
import { create as createInvoice } from '../entites/invoice.js';

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
    // GİB Portal API'ye gönderim yapan yardımcı fonksiyon
    async function sendInvoiceToGibById(invoiceId) {
        // Id'yi garanti sayıya çevir
        const numericId = Number(invoiceId);
        if (!Number.isFinite(numericId) || numericId <= 0) {
            throw new Error("Geçersiz invoiceId: " + invoiceId);
        }

        const baseUrl = window.gibPortalApiBaseUrl || window.gibPortalApiBaseUrl;
        if (!baseUrl) {
            throw new Error("GİB Portal API Base URL (gibPortalApiBaseUrl) tanımlı değil.");
        }

        // BaseUrl sonu / ile bitmiyorsa ekle
        const normBase = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";

        // Senin Swagger URL formatın:
        // https://localhost:7151/api/TurkcellEFatura/einvoice/send-json/11?isExport=false
        const url = normBase + "TurkcellEFatura/einvoice/send-json/" + encodeURIComponent(numericId) + "?isExport=false";

        console.log("[GIB] Gönderim URL:", url);

        // Genelde bu endpoint POST oluyor; body istemiyor, sadece route + query ile çalışıyor
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Accept": "application/json"
            }
            // body yok
        });

        if (!response.ok) {
            // JSON dönmezse bile hata mesajını görmek için text'i de okumaya çalış
            let errorText = "";
            try {
                errorText = await response.text();
            } catch { /* ignore */ }

            throw new Error("GİB API isteği başarısız. Status: "
                + response.status + " " + response.statusText
                + (errorText ? " - " + errorText : ""));
        }

        // Bazı endpoint'ler 204 NoContent dönebilir, ona göre koru
        let res = null;
        try {
            res = await response.json();
        } catch {
            res = {};
        }
        console.log(res);
        return res; // Örn: { statusCode: 200, uuid: '...', invoiceNumber: '...', documentId: '...' }
    }


    // RowVersion hex → base64 (SQL rowversion -> JSON string)
    function rowVersionHexToBase64(hex) {
        if (!hex) return "";
        let h = String(hex).trim();
        if (h.startsWith("0x") || h.startsWith("0X")) {
            h = h.substring(2);
        }
        if (h.length % 2 === 1) {
            h = "0" + h;
        }
        const bytes = [];
        for (let i = 0; i < h.length; i += 2) {
            const b = parseInt(h.substr(i, 2), 16);
            if (!isNaN(b)) bytes.push(b);
        }
        let bin = "";
        for (let i = 0; i < bytes.length; i++) {
            bin += String.fromCharCode(bytes[i]);
        }
        // btoa tarayıcıda hazır
        try {
            return btoa(bin);
        } catch (e) {
            console.error("RowVersion base64 dönüşümünde hata:", e);
            return "";
        }
    }

    // BaseEntity alanlarını doldurmak için helper
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

    // Global ListData objesini garanti altına al
    global.ListData = global.ListData || {};

    function getListData() {
        return global.ListData || {};
    }

    function generateUUIDv4() {
        // GİB'in ETTN formatına uygun UUID v4
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        }).toUpperCase();
    }

    function showAlert(type, message) {
        // type: "success" | "danger"
        const $form = $("#faturaformu");
        if (!$form.length) return;

        $(".invoice-alert").remove();

        const $alert = $(`
            <div class="alert alert-${type} invoice-alert" role="alert" style="margin-bottom:15px;">
                ${message}
            </div>
        `);

        $form.prepend($alert);

        setTimeout(function () {
            $alert.fadeOut(400, function () { $(this).remove(); });
        }, 5000);
    }

    /***********************************************************
     *  GLOBAL STATE
     ***********************************************************/
    const UIState = {
        selectedCustomerId: null
    };

    // invoiceModel – tüm sekmeleri tutan client-side model
    const invoiceModel = {
        invoiceheader: {
            Invoice_ID: null,        // ETTN
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
            TemplateName: null
        },

        // Önizleme için nested customerParty
        customer: null,

        // Modellerin düz halleri (DB/DTO için daha uygun)
        Customer: null,          // flat customer (IdentificationID vs.)
        payment: null,           // Payment sekmesi
        kamuAlicisi: null,       // Kamu alıcısı
        sgk: null,               // SGK ek alanlar
        irsaliyeler: [],         // İrsaliye listesi

        // Ek alanlar
        sellerAdditional: [],        // Satıcı Ek
        sellerAgentAdditional: [],   // Satıcı Şube Ek
        buyerAdditional: [],         // Alıcı Ek
        ekAlan: [],                  // KOBİ / EYDEP vb.

        // Fatura satırları
        invoiceLines: []
    };

    global.invoiceModel = invoiceModel;
    global.Utils = Utils;
    global.UIState = UIState;

    /***********************************************************
     *  TEST MÜŞTERİLER (Demo için)
     ***********************************************************/
    const TestCustomers = [
        {
            id: 1,
            displayName: "0001 - TEST A.Ş.",
            identificationId: "1234567803",
            aliciEtiketi: "urn:mail:testfirma@example.com",
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
        },
        {
            id: 2,
            displayName: "0002 - DENEME ŞTİ.",
            identificationId: "1234567803",
            aliciEtiketi: "urn:mail:deneme@example.com",
            partyName: "DENEME LTD. ŞTİ.",
            firstName: "DENEME",
            lastName: "YETKİLİ",
            taxSchemeName: "ANKARA VERGİ DAİRESİ",
            ulkeCode: "TR",
            ulkeText: "Türkiye",
            ilCode: "06",
            ilName: "ANKARA",
            ilceCode: "0601",
            ilceName: "ÇANKAYA",
            streetName: "DENEME MAH. DENEME CAD. NO:2",
            email: "info@deneme.com",
            postalCode: "06000",
            website: "https://www.deneme.com",
            telephone: "03120000000",
            fax: ""
        }
    ];

    global.TestCustomers = TestCustomers;

    /***********************************************************
     *  LİSTELERİ DOLDURMA (window.ListData)
     ***********************************************************/
    function fillSubeDropdown() {
        const listData = getListData();
        const subeList = listData.subeList || [];
        const $ddl = $("#ddlSubeKodu");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", "0"));

        subeList.forEach(s => {
            const text = s.SubeAdi || ("Şube " + s.SubeKodu);
            const opt = new Option(text, s.SubeKodu);
            $(opt).attr("data-firmaid", s.FirmaId);
            $ddl.append(opt);
        });

        if (subeList.length > 0) {
            $ddl.val(subeList[0].SubeKodu).trigger("change");
        }
    }

    function fillSourceUrnDropdown() {
        const listData = getListData();
        const arr = listData.GondericiEtiketList || [];
        const $ddl = $("#ddlSourceUrn");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", ""));

        arr.forEach(urn => {
            $ddl.append(new Option(urn, urn));
        });

        if (arr.length > 0) {
            $ddl.val(arr[0]).trigger("change");
        }
    }

    function fillInvoicePrefixDropdown() {
        const listData = getListData();
        const prefixes = listData.InvoicePrefixList || listData.invoicePrefixList || [];
        const $ddl = $("#ddlInvoicePrefix");
        if (!$ddl.length) return;

        $ddl.empty();

        if (!prefixes.length) {
            $ddl.append(new Option("FTR", "FTR"));
            $ddl.append(new Option("IRS", "IRS"));
        } else {
            prefixes.forEach(p => {
                if (typeof p === "string") {
                    $ddl.append(new Option(p, p));
                } else if (p && p.Kodu) {
                    $ddl.append(new Option(p.Kodu, p.Kodu));
                }
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

        arr.forEach(x => {
            $ddl.append(new Option(x, x));
        });
    }

    function fillAliciEtiketiDropdown() {
        const listData = getListData();
        const arr = listData.KurumEtiketList || [];
        const $ddl = $("#ddlAliciEtiketi");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", ""));

        arr.forEach(urn => {
            $ddl.append(new Option(urn, urn));
        });

        if (arr.length > 0) {
            $ddl.val(arr[0]).trigger("change");
        }
    }

    function fillParaBirimiDropdown() {
        const listData = getListData();
        const paraList = listData.parabirimList || [];
        const $ddl = $("#ddlParaBirimi");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append(new Option("Seçiniz", ""));

        paraList.forEach(p => {
            $ddl.append(new Option(`${p.Kodu} - ${p.Aciklama}`, p.Kodu));
        });

        if (paraList.some(p => p.Kodu === "TRY")) {
            $ddl.val("TRY");
        }
    }

    function fillPaymentDropdowns() {
        const listData = getListData();

        // Ödeme Şekli
        const $means = $("#PaymentMeansCode");
        if ($means.length) {
            $means.empty();
            $means.append(new Option("Seçiniz", ""));
            (listData.OdemeList || []).forEach(o => {
                $means.append(new Option(`${o.OdemeKodu} - ${o.Aciklama}`, o.OdemeKodu));
            });
        }

        // Ödeme Kanalı
        const $channel = $("#PaymentChannelCode");
        if ($channel.length) {
            $channel.empty();
            $channel.append(new Option("Seçiniz", ""));
            (listData.OdemeKanalList || []).forEach(o => {
                $channel.append(new Option(`${o.OdemeKanalKodu} - ${o.Aciklama}`, o.OdemeKanalKodu));
            });
        }

        // Hesap Para Birimi
        const $payeeCur = $("#PayeeFinancialCurrencyCode");
        if ($payeeCur.length) {
            $payeeCur.empty();
            $payeeCur.append(new Option("Seçiniz", ""));
            (listData.parabirimList || []).forEach(p => {
                $payeeCur.append(new Option(`${p.Kodu} - ${p.Aciklama}`, p.Kodu));
            });

            if ((listData.parabirimList || []).some(p => p.Kodu === "TRY")) {
                $payeeCur.val("TRY");
            }
        }
    }

    /***********************************************************
     *  MANUEL ŞEHİR (chkManuelSehir)
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
     *  ÖDEME TABI - Alıcı Bilgilerinden Kopyala
     ***********************************************************/
    function copyAliciToKamu() {
        const identificationId = $("#txtIdentificationID").val() || "";
        const partyName = $("#txtPartyName").val() || "";

        let ulke = "";
        let il = "";

        if ($("#chkManuelSehir").is(":checked")) {
            ulke = $("#txtUlke").val() || "";
            il = $("#txtIl").val() || "";
        } else {
            ulke = $("#ddlUlke option:selected").text() || "";
            il = $("#ddlIl option:selected").text() || "";
        }

        $("#txtKamuVkn").val(identificationId);
        $("#txtKamuUnvan").val(partyName);
        $("#txtKamuUlke").val(ulke);
        $("#txtKamuIl").val(il);
    }

    /***********************************************************
     *  TEST MÜŞTERİ LISTESİ (Ara)
     ***********************************************************/
    function fillMusteriAraDropdown() {
        const $ddl = $("#ddlMusteriAra");
        if (!$ddl.length) return;

        $ddl.empty();
        $ddl.append($("<option>", { value: "", text: "Seçiniz" }));

        TestCustomers.forEach(c => {
            $ddl.append(
                $("<option>", {
                    value: c.id,
                    text: c.displayName
                })
            );
        });
    }

    // Alıcıyı formdan okuyan ortak helper (flat Customer)
    function readCustomerFromUI() {
        const isManual = $("#chkManuelSehir").is(":checked");

        const IdentificationID = $("#txtIdentificationID").val() || "";
        const PartyName = $("#txtPartyName").val() || "";
        const FirstName = $("#txtPersonFirstName").val() || "";
        const LastName = $("#txtPersonLastName").val() || "";
        const TaxOffice = $("#txtTaxSchemeName").val() || "";

        const CountryName = isManual
            ? ($("#txtUlke").val() || "")
            : ($("#ddlUlke option:selected").text() || "");

        const CityName = isManual
            ? ($("#txtIl").val() || "")
            : ($("#ddlIl option:selected").text() || "");

        const CitySubdivisionName = isManual
            ? ($("#txtIlce").val() || "")
            : ($("#ddlIlce option:selected").text() || "");

        const StreetName = $("#txtStreetName").val() || "";
        const Email = $("#txtEmail").val() || "";
        const WebsiteURI = $("#txtWebsite").val() || "";
        const Telephone = $("#txtTelephone").val() || "";
        const Fax = $("#txtFax").val() || "";
        const PostalZone = $("#txtPostalCode").val() || "";
        const AliciEtiketi = $("#ddlAliciEtiketi").val() || "";

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
            AliciEtiketi,
            ManuelCityEntry: isManual
        };
    }

    // FirstName/LastName boşsa PartyName'den ad/soyad ayır
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

    function fillCustomerFieldsFromTest(cust) {
        if (!cust) return;

        UIState.selectedCustomerId = cust.id;

        $("#txtIdentificationID").val(cust.identificationId);
        $("#ddlAliciEtiketi").val(cust.aliciEtiketi || "").trigger("change");
        $("#txtPartyName").val(cust.partyName);

        let firstName = cust.firstName || "";
        let lastName = cust.lastName || "";

        if (!firstName && !lastName && cust.partyName) {
            const parts = cust.partyName.trim().split(/\s+/);
            if (parts.length === 1) {
                firstName = parts[0];
                lastName = ".";
            } else {
                lastName = parts.pop();
                firstName = parts.join(" ");
            }
        }

        $("#txtPersonFirstName").val(firstName);
        $("#txtPersonLastName").val(lastName);

        $("#txtTaxSchemeName").val(cust.taxSchemeName);
        $("#ddlUlke").val(cust.ulkeCode).trigger("change");
        $("#txtStreetName").val(cust.streetName);
        $("#txtEmail").val(cust.email);
        $("#txtPostalCode").val(cust.postalCode);
        $("#txtWebsite").val(cust.website);
        $("#txtTelephone").val(cust.telephone);
        $("#txtFax").val(cust.fax);

        setTimeout(() => { $("#ddlIl").val(cust.ilCode).trigger("change"); }, 50);
        setTimeout(() => { $("#ddlIlce").val(cust.ilceCode).trigger("change"); }, 100);

        // invoiceModel.customer (xml/preview için nested)
        invoiceModel.customer = {
            customerParty: {
                IdentificationID: cust.identificationId,
                PartyName: cust.partyName,
                TaxSchemeName: cust.taxSchemeName,
                CountryName: cust.ulkeText,
                CityName: cust.ilName,
                CitySubdivisionName: cust.ilceName,
                StreetName: cust.streetName,
                PostalZone: cust.postalCode,
                ElectronicMail: cust.email,
                Telephone: cust.telephone,
                WebsiteURI: cust.website,
                Person_FirstName: firstName,
                Person_FamilyName: lastName,
                ManuelCityAndSubdivision: $("#chkManuelSehir").is(":checked")
            }
        };

        // Flat Customer modeli de dolduralım
        invoiceModel.Customer = readCustomerFromUI();
    }

    /***********************************************************
     *  HEADER INPUT BINDING
     ***********************************************************/
    function bindHeaderInputs() {
        $(document).on("change", "#ddlSubeKodu", function () {
            invoiceModel.invoiceheader.BranchCode = $(this).val();
        });

        $(document).on("change", "#ddlSourceUrn", function () {
            invoiceModel.invoiceheader.SourceUrn = $(this).val();
        });

        $(document).on("change", "#ddlInvoicePrefix", function () {
            invoiceModel.invoiceheader.Prefix = $(this).val();
        });

        $(document).on("change", "#ddlGoruntuDosyasi", function () {
            invoiceModel.invoiceheader.TemplateName = $(this).val();
        });

        $(document).on("change", "#ddlParaBirimi", function () {
            const cur = $(this).val();
            invoiceModel.invoiceheader.DocumentCurrencyCode = cur;
            invoiceModel.invoiceheader.Currency = cur;
        });

        $(document).on("change", "#ddlSenaryo", function () {
            invoiceModel.invoiceheader.Scenario = $(this).val();
        });

        $(document).on("change", "#ddlFaturaTipi", function () {
            invoiceModel.invoiceheader.InvoiceTypeCode = $(this).val();
            toggleIstisnaOnLines();
        });
    }

    /***********************************************************
     *  BİRİM / İSTİSNA SELECTLERİ
     ***********************************************************/
    function fillBirimSelect($select) {
        const listData = getListData();
        const birimList = listData.birimList || [];
        if (!$select.length || !birimList.length) return;

        $select.empty();
        $select.append($("<option>", { value: "", text: "Seçiniz" }));
        birimList.forEach(b => {
            $select.append(
                $("<option>", {
                    value: b.BirimKodu,
                    text: `${b.BirimKodu} - ${b.Aciklama}`
                })
            );
        });
    }

    function fillIstisnaSelect($select) {
        const listData = getListData();
        const istisnaList = listData.istisnaList || [];
        if (!$select.length || !istisnaList.length) return;

        $select.empty();
        $select.append($("<option>", { value: "", text: "Seçiniz" }));
        istisnaList.forEach(i => {
            $select.append(
                $("<option>", {
                    value: i.Kodu,
                    text: `${i.Kodu} - ${i.Adi}`
                })
            );
        });
    }

    function toggleIstisnaOnLines() {
        const isIstisna = $("#ddlFaturaTipi").val() === "ISTISNA";
        $("#manuel_grid tbody tr").each(function () {
            const $istisna = $(this).find(".js-line-istisna");
            if (isIstisna) {
                $istisna.prop("disabled", false);
                if ($istisna.children().length <= 1) {
                    fillIstisnaSelect($istisna);
                }
            } else {
                $istisna.prop("disabled", true).val("");
            }
        });
    }

    /***********************************************************
     *  Fatura Kalemleri – Satır Template & Ekle / Sil
     ***********************************************************/
    function createLineRowTemplate() {
        return `
        <tr>
            <td class="text-center"><span class="js-line-index"></span></td>
            <td>
                <input type="text" class="form-control input-sm js-line-name" placeholder="Mal/Hizmet Adı">
            </td>
            <td>
                <input type="number" step="0.0001" class="form-control input-sm js-line-qty" value="1">
            </td>
            <td>
                <select class="form-control input-sm js-line-unit"></select>
            </td>
            <td>
                <input type="number" step="0.0001" class="form-control input-sm js-line-price" value="0">
            </td>
            <td>
                <input type="number" step="0.01" class="form-control input-sm js-line-disc-rate" value="0">
            </td>
            <td>
                <input type="text" class="form-control input-sm js-line-disc-amount" value="0,00" disabled>
            </td>
            <td>
                <input type="text" class="form-control input-sm js-line-amount" value="0,00" disabled>
            </td>
            <td>
                <input type="number" step="0.01" class="form-control input-sm js-line-kdv" value="20">
            </td>
            <td>
                <input type="text" class="form-control input-sm js-line-kdv-amount" value="0,00" disabled>
            </td>
            <td>
                <select class="form-control input-sm js-line-istisna" disabled></select>
            </td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-line-remove">
                    <i class="fa fa-trash"></i>
                </button>
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
     *  SATIR HESAPLAMALARI + TOPLAMLAR
     ***********************************************************/
    function calculateRowValues(qty, unitPrice, discRate, kdvRate, isKdvDahil) {
        qty = qty || 0;
        unitPrice = unitPrice || 0;
        discRate = discRate || 0;
        kdvRate = kdvRate || 0;

        let indirim = 0;
        let matrah = 0;
        let kdv = 0;
        let toplam = 0;

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
        let toplamMalHizmet = 0;
        let toplamIskonto = 0;
        let kdvMatrah = 0;
        let kdvTutar = 0;
        let vergilerDahil = 0;

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
     *  ÖNİZLEME MODEL / HTML
     ***********************************************************/
    function buildInvoiceModelFromUI() {
        // invoiceModel’in kopyası
        const model = JSON.parse(JSON.stringify(invoiceModel));

        const ettn = $("#txtUUID").val() || null;

        model.invoiceheader.Invoice_ID = ettn;
        model.invoiceheader.IssueDate = $("#txtIssueDate").val() || null;
        model.invoiceheader.IssueTime = $("#txtIssueTime").val() || null;
        model.invoiceheader.Currency = $("#ddlParaBirimi").val() || model.invoiceheader.Currency;
        model.invoiceheader.DocumentCurrencyCode = model.invoiceheader.Currency;
        model.invoiceheader.Note = $("#Note").val() || null;
        model.invoiceheader.InvoiceTypeCode = $("#ddlFaturaTipi").val() || model.invoiceheader.InvoiceTypeCode;
        model.invoiceheader.Scenario = $("#ddlSenaryo").val() || model.invoiceheader.Scenario;
        model.invoiceheader.BranchCode = $("#ddlSubeKodu").val() || null;
        model.invoiceheader.SourceUrn = $("#ddlSourceUrn").val() || null;
        model.invoiceheader.Prefix = $("#ddlInvoicePrefix").val() || null;
        model.invoiceheader.TemplateName = $("#ddlGoruntuDosyasi").val() || null;

        // -------- ALICI (Customer) ----------
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

        // -------- İRSALİYE BİLGİLERİ ----------
        model.irsaliyeler = [];
        $("#tblIrsaliyeBody tr").each(function () {
            const no = ($(this).find(".irs-no").val() || "").trim();
            const date = ($(this).find(".irs-date").val() || "").trim();
            if (no || date) {
                model.irsaliyeler.push({
                    IrsaliyeNo: no,
                    IrsaliyeTarihi: date
                });
            }
        });

        // -------- ÖDEME ----------
        const payment = {
            PaymentMeansCode: $("#PaymentMeansCode").val() || "",
            PaymentDueDate: $("#PaymentDueDate").val() || "",
            InstructionNote: $("#InstructionNote").val() || "",
            PaymentChannelCode: $("#PaymentChannelCode").val() || "",
            PayeeFinancialAccount: $("#PayeeFinancialAccount").val() || "",
            PayeeFinancialCurrencyCode: $("#PayeeFinancialCurrencyCode").val() || ""
        };
        const paymentHasData = Object.values(payment).some(v => (v || "").toString().trim() !== "");
        model.payment = paymentHasData ? payment : null;

        // -------- KAMU ALICISI ----------
        const kamu = {
            Vkn: $("#txtKamuVkn").val() || "",
            Unvan: $("#txtKamuUnvan").val() || "",
            Ulke: $("#txtKamuUlke").val() || "",
            Il: $("#txtKamuIl").val() || ""
        };
        const kamuHasData = Object.values(kamu).some(v => (v || "").toString().trim() !== "");
        model.kamuAlicisi = kamuHasData ? kamu : null;

        // -------- SGK EK ALANLAR ----------
        const ilaveFaturaTipi = $("#ddlilaveFaturaTipi").val() || "";
        if (ilaveFaturaTipi) {
            model.sgk = {
                IlaveFaturaTipi: ilaveFaturaTipi,
                MukellefKodu: $("#mukellefkodu").val() || "",
                MukellefAdi: $("#mukellefadi").val() || "",
                DosyaNo: $("#dosyano").val() || "",
                GonderimTarihiBaslangic: $("#txtSendingDateBaslangic").val() || "",
                GonderimTarihiBitis: $("#txtSendingDateBitis").val() || ""
            };
        } else {
            model.sgk = null;
        }

        // -------- EK ALANLAR (Satıcı / Şube / Alıcı / KOBİ-EYDEP) ----------
        model.sellerAdditional = [];
        $("#tbodySaticiEk tr").each(function () {
            const key = ($(this).find(".js-satici-kod").val() || "").trim();
            const val = ($(this).find(".js-satici-deger").val() || "").trim();
            if (key || val) {
                model.sellerAdditional.push({ Key: key, Value: val });
            }
        });

        model.sellerAgentAdditional = [];
        $("#tbodySaticiSubeEk tr").each(function () {
            const key = ($(this).find(".js-satici-sube-kod").val() || "").trim();
            const val = ($(this).find(".js-satici-sube-deger").val() || "").trim();
            if (key || val) {
                model.sellerAgentAdditional.push({ Key: key, Value: val });
            }
        });

        model.buyerAdditional = [];
        $("#tbodyAliciEk tr").each(function () {
            const key = ($(this).find(".js-alici-kod").val() || "").trim();
            const val = ($(this).find(".js-alici-deger").val() || "").trim();
            if (key || val) {
                model.buyerAdditional.push({ Key: key, Value: val });
            }
        });

        model.ekAlan = [];
        $("#tbodyEkalani tr").each(function () {
            const type = ($(this).find(".js-ekalan-type").val() || "").trim();
            const answer = ($(this).find(".js-ekalan-answer").val() || "").trim();
            const info = ($(this).find(".js-ekalan-info").val() || "").trim();
            const date = ($(this).find(".js-ekalan-date").val() || "").trim();
            if (type || answer || info || date) {
                model.ekAlan.push({
                    Type: type,
                    Answer: answer,
                    Info: info,
                    Date: date
                });
            }
        });

        // -------- SATIRLAR ----------
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
                SellerItemCode: "" // UI'de alan yoksa boş
            });
        });

        return model;
    }

    // Fatura önizleme için PDF'e benzer stil tanımı
    let previewStylesInjected = false;
    function ensurePreviewStyles() {
        if (previewStylesInjected) return;

        const css = `
#invoicePreviewModal .modal-dialog {
    max-width: 900px;
    width: 95%;
}

#invoicePreviewContent {
    background: #f5f5f5;
    padding: 10px;
}

#invoicePreviewContent .invoice-pdf {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 11px;
    color: #000;
}

#invoicePreviewContent .invoice-pdf-page {
    background: #ffffff;
    margin: 0 auto;
    border: 1px solid #000;
    padding: 8px 12px 12px 12px;
    position: relative;
    max-width: 900px;
    min-height: 600px;
}

#invoicePreviewContent .invoice-watermark {
    position: absolute;
    top: 45%;
    left: 50%;
    transform: translate(-50%, -50%) rotate(-30deg);
    font-size: 80px;
    font-weight: 700;
    color: rgba(255,0,0,0.18);
    z-index: 0;
    pointer-events: none;
}

#invoicePreviewContent .invoice-qr {
    width: 95px;
    height: 95px;
    border: 1px solid #000;
    display: inline-block;
    background: #fff;
    font-size: 10px;
    text-align: center;
    line-height: 95px;
}

#invoicePreviewContent .invoice-logo-text {
    font-size: 22px;
    font-weight: bold;
    margin-top: 15px;
}

#invoicePreviewContent .invoice-table-tight th,
#invoicePreviewContent .invoice-table-tight td {
    padding: 3px 4px !important;
    border: 1px solid #000 !important;
}

#invoicePreviewContent .invoice-table-no-border th,
#invoicePreviewContent .invoice-table-no-border td {
    padding: 2px 4px !important;
    border: none !important;
}

#invoicePreviewContent .invoice-separator {
    border-top: 1px solid #000;
    margin: 6px 0 8px 0;
}

#invoicePreviewContent .text-xs {
    font-size: 10px;
}

#invoicePreviewContent .invoice-footer-page {
    text-align: center;
    margin-top: 8px;
    font-size: 10px;
}
    `;

        $("head").append('<style id="invoicePdfPreviewStyles">' + css + '</style>');
        previewStylesInjected = true;
    }


    function renderInvoicePreview(model) {
        ensurePreviewStyles();

        const h = model.invoiceheader || {};
        const c = model.customer && model.customer.customerParty ? model.customer.customerParty : {};

        const lines = model.invoiceLines || [];

        const currencyCode = h.DocumentCurrencyCode || "TRY";
        const currencySuffix = currencyCode === "TRY" ? "TL" : currencyCode;

        let invoiceNo =
            $("#txtInvoiceNumber").val() ||
            $("#txtFaturaNo").val() ||
            "";

        if (!invoiceNo) {
            if (h.Prefix && h.Invoice_ID) {
                invoiceNo = h.Prefix + (h.Invoice_ID.replace(/-/g, "").substring(0, 13));
            } else {
                invoiceNo = h.Invoice_ID || "";
            }
        }

        function formatIssueDate(dateStr) {
            if (!dateStr) return "";
            const parts = dateStr.split("-");
            if (parts.length === 3) {
                return parts[2] + " - " + parts[1] + " - " + parts[0];
            }
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
        lines.forEach(l => {
            toplamIskonto += Number(l.DiscountAmount || 0);
        });

        const malHizmetToplam = Number(h.LineExtensionAmount || 0);
        const kdvToplam = Number(h.Taxes || 0);
        const vergilerDahil = Number(h.TaxInclusiveAmount || h.PayableAmount || (malHizmetToplam + kdvToplam));
        const odenecek = Number(h.PayableAmount || vergilerDahil);

        const kdvRates = Array.from(new Set(
            lines
                .map(l => Number(l.KdvOran))
                .filter(v => !isNaN(v))
        ));
        let kdvLabelSuffix = "";
        if (kdvRates.length === 1) {
            kdvLabelSuffix = " (%" + Utils.formatNumber(kdvRates[0], 0) + ")";
        }

        const aliciAdSoyad = ((c.Person_FirstName || "") + " " + (c.Person_FamilyName || "")).trim() ||
            (c.PartyName || "");

        const aliciAdresSatir1 = c.StreetName || "";
        const aliciAdresSatir2 = [
            c.PostalZone || "",
            c.CitySubdivisionName || "",
            c.CityName || ""
        ].filter(Boolean).join(" ");

        function fmt(n, dec) {
            return Utils.formatNumber(n || 0, dec == null ? 2 : dec);
        }

        const issueDateText = formatIssueDate(h.IssueDate);
        const issueTimeText = formatIssueTime(h.IssueTime);

        const ozellestirmeNo = $("#txtOzelNitelikNo").val() || "TR1.2";

        let html = "";
        html += "<div class='invoice-pdf'>";
        html += "  <div class='invoice-pdf-page'>";

        html += "    <div class='invoice-watermark'>TASLAK</div>";

        html += "    <div class='row' style='position:relative; z-index:1;'>";
        html += "      <div class='col-xs-6'>";
        html += "        <table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "          <tr><td><strong>" + ($("#sellerName, #lblSellerName, #txtSellerName, #txtUnvan").first().text().trim() ||
            $("#sellerName, #lblSellerName, #txtSellerName, #txtUnvan").first().val() || "Satıcı Ünvanı") + "</strong></td></tr>";
        html += "          <tr><td>" + ($("#sellerAddress").text().trim() || $("#sellerAddress").val() || "") + "</td></tr>";
        html += "          <tr><td>" + ($("#sellerDistrictCity").text().trim() || "") + "</td></tr>";
        html += "          <tr><td>Tel:" + ($("#sellerPhone").val() || $("#sellerPhone").text() || "") +
            "  Fax:" + ($("#sellerFax").val() || $("#sellerFax").text() || "") + "</td></tr>";
        html += "          <tr><td>Web Sitesi:" + ($("#sellerWeb").val() || $("#sellerWeb").text() || "") + "</td></tr>";
        html += "          <tr><td>E-Posta:" + ($("#sellerEmail").val() || $("#sellerEmail").text() || "") + "</td></tr>";
        html += "          <tr><td>Vergi Dairesi:" + ($("#sellerVergiDairesi").val() || $("#sellerVergiDairesi").text() || "") + "</td></tr>";
        html += "          <tr><td>VKN:" + ($("#sellerVkn").val() || $("#sellerVkn").text() || "") + "</td></tr>";
        html += "          <tr><td>TICARETSICILNO:" + ($("#sellerTicaretSicilNo").val() || $("#sellerTicaretSicilNo").text() || "") + "</td></tr>";
        html += "          <tr><td>MERSISNO:" + ($("#sellerMersisNo").val() || $("#sellerMersisNo").text() || "") + "</td></tr>";
        html += "        </table>";
        html += "      </div>";

        html += "      <div class='col-xs-3 text-center'>";
        html += "        <div class='invoice-logo-text'>e-FATURA</div>";
        html += "      </div>";

        html += "      <div class='col-xs-3 text-right'>";
        html += "        <div class='invoice-qr'>QR KOD</div>";
        html += "      </div>";
        html += "    </div>";

        html += "    <div class='row' style='position:relative; z-index:1; margin-top:4px;'>";
        html += "      <div class='col-xs-12 text-right text-xs'><strong>ETTN:</strong> " + (h.Invoice_ID || "") + "</div>";
        html += "    </div>";

        html += "    <hr class='invoice-separator'/>";

        html += "    <div class='row' style='position:relative; z-index:1;'>";
        html += "      <div class='col-xs-7'>";
        html += "        <table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "          <tr><td><strong>SAYIN</strong></td></tr>";
        html += "          <tr><td><strong>" + (aliciAdSoyad || "") + "</strong></td></tr>";
        if (aliciAdresSatir1) {
            html += "      <tr><td>" + aliciAdresSatir1 + "</td></tr>";
        }
        if (aliciAdresSatir2) {
            html += "      <tr><td>" + aliciAdresSatir2 + "</td></tr>";
        }
        if (c.WebsiteURI) {
            html += "      <tr><td>Web Sitesi:" + c.WebsiteURI + "</td></tr>";
        }
        if (c.ElectronicMail) {
            html += "      <tr><td>E-Posta:" + c.ElectronicMail + "</td></tr>";
        }
        if (c.Telephone) {
            html += "      <tr><td>Tel:" + c.Telephone + "</td></tr>";
        }
        if (c.TaxSchemeName) {
            html += "      <tr><td>Vergi Dairesi:" + c.TaxSchemeName + "</td></tr>";
        }
        if (c.IdentificationID) {
            html += "      <tr><td>VKN/TCKN:" + c.IdentificationID + "</td></tr>";
        }
        html += "        </table>";
        html += "      </div>";

        html += "      <div class='col-xs-5'>";
        html += "        <table class='table table-condensed invoice-table-tight text-xs'>";
        html += "          <tr><th>Özelleştirme No</th><td>" + ozellestirmeNo + "</td></tr>";
        html += "          <tr><th>Senaryo</th><td>" + (h.Scenario || "") + "</td></tr>";
        html += "          <tr><th>Fatura Tipi</th><td>" + (h.InvoiceTypeCode || "") + "</td></tr>";
        html += "          <tr><th>Fatura No</th><td>" + (invoiceNo || "") + "</td></tr>";
        html += "          <tr><th>Fatura Tarihi</th><td>" + (issueDateText || "") + "</td></tr>";
        html += "          <tr><th>Fatura Saati</th><td>" + (issueTimeText || "") + "</td></tr>";
        html += "        </table>";
        html += "      </div>";
        html += "    </div>";

        html += "    <div class='row' style='position:relative; z-index:1; margin-top:4px;'>";
        html += "      <div class='col-xs-12'>";
        html += "        <table class='table table-condensed invoice-table-tight text-xs'>";
        html += "          <thead>";
        html += "            <tr>";
        html += "              <th style='width:3%;'>Sıra No</th>";
        html += "              <th style='width:18%;'>Mal Hizmet</th>";
        html += "              <th style='width:10%;'>Satıcı Ürün Kodu</th>";
        html += "              <th style='width:8%;'>Miktar</th>";
        html += "              <th style='width:9%;'>Birim Fiyat</th>";
        html += "              <th style='width:8%;'>İskonto Oranı</th>";
        html += "              <th style='width:9%;'>İskonto Tutarı</th>";
        html += "              <th style='width:6%;'>KDV Oranı</th>";
        html += "              <th style='width:9%;'>KDV Tutarı</th>";
        html += "              <th style='width:9%;'>Diğer Vergiler</th>";
        html += "              <th style='width:11%;'>Mal Hizmet Tutarı</th>";
        html += "            </tr>";
        html += "          </thead>";
        html += "          <tbody>";

        if (!lines.length) {
            html += "        <tr><td colspan='11' class='text-center'>Herhangi bir satır bulunamadı.</td></tr>";
        } else {
            lines.forEach(l => {
                const discRateText = fmt(l.DiscountRate || 0, 2);
                html += "        <tr>";
                html += "          <td class='text-right'>" + (l.LineNo || "") + "</td>";
                html += "          <td>" + (l.ItemName || "") + "</td>";
                html += "          <td>" + (l.SellerItemCode || "") + "</td>";
                html += "          <td class='text-right'>" + fmt(l.Quantity || 0, 4) + " " + (l.UnitCode || "") + "</td>";
                html += "          <td class='text-right'>" + fmt(l.Price || 0, 2) + currencySuffix + "</td>";
                html += "          <td class='text-right'>%" + discRateText + "</td>";
                html += "          <td class='text-right'>" + fmt(l.DiscountAmount || 0, 2) + currencySuffix + "</td>";
                html += "          <td class='text-right'>" + fmt(l.KdvOran || 0, 0) + "</td>";
                html += "          <td class='text-right'>" + fmt(l.KdvTutar || 0, 2) + currencySuffix + "</td>";
                html += "          <td class='text-right'>" + fmt(0, 2) + currencySuffix + "</td>";
                html += "          <td class='text-right'>" + fmt(l.Amount || 0, 2) + currencySuffix + "</td>";
                html += "        </tr>";
            });
        }

        html += "          </tbody>";
        html += "        </table>";
        html += "      </div>";
        html += "    </div>";

        html += "    <div class='row' style='position:relative; z-index:1; margin-top:4px;'>";
        html += "      <div class='col-xs-6'>";
        if (h.Note) {
            html += "      <p class='text-xs'><strong>Not:</strong> " + (h.Note || "") + "</p>";
        }
        html += "      </div>";

        html += "      <div class='col-xs-6'>";
        html += "        <table class='table table-condensed invoice-table-no-border text-xs'>";
        html += "          <tr><th>Mal Hizmet Toplam Tutarı</th><td class='text-right'>" + fmt(malHizmetToplam, 2) + currencySuffix + "</td></tr>";
        html += "          <tr><th>Toplam İskonto</th><td class='text-right'>" + fmt(toplamIskonto, 2) + currencySuffix + "</td></tr>";
        html += "          <tr><th>KDV Matrahı" + kdvLabelSuffix + "</th><td class='text-right'>" + fmt(malHizmetToplam, 2) + currencySuffix + "</td></tr>";
        html += "          <tr><th>Hesaplanan KDV" + kdvLabelSuffix + "</th><td class='text-right'>" + fmt(kdvToplam, 2) + currencySuffix + "</td></tr>";
        html += "          <tr><th>Vergiler Dahil Toplam Tutar</th><td class='text-right'>" + fmt(vergilerDahil, 2) + currencySuffix + "</td></tr>";
        html += "          <tr><th>Ödenecek Tutar</th><td class='text-right'><strong>" + fmt(odenecek, 2) + currencySuffix + "</strong></td></tr>";
        html += "        </table>";
        html += "      </div>";
        html += "    </div>";

        html += "    <div class='invoice-footer-page'>1/1</div>";

        html += "  </div>";
        html += "</div>";

        $("#invoicePreviewContent").html(html);
    }

    /***********************************************************
     *  Invoice Entity (DTO) - C# Invoice ve tüm ilişkiler full dolu
     ***********************************************************/
    /***********************************************************
 *  Invoice Entity (DTO) - C# Invoice ve tüm ilişkiler full dolu
 ***********************************************************/
    function buildInvoiceEntityFromUI() {
        // Toplamları güncelle
        recalcTotals();
        const model = buildInvoiceModelFromUI();
        const h = model.invoiceheader || {};

        // Seçili müşteri Id (varsa)
        let customerId = 0;
        const ddlVal = $("#ddlMusteriAra").val();
        if (ddlVal) {
            const parsed = parseInt(ddlVal, 10);
            if (!isNaN(parsed)) customerId = parsed;
        }

        // Kullanıcı Id'si – sayfada varsa oradan al, yoksa 0
        let currentUserId = 0;
        const $userHidden = $("#hdnUserId");
        if ($userHidden.length) {
            const parsed = parseInt($userHidden.val(), 10);
            if (!isNaN(parsed)) currentUserId = parsed;
        }

        const nowIso = new Date().toISOString();

        // Sabit RowVersion hex (yeni kayıtlar için)
        const DEFAULT_ROWVERSION_HEX = "0x00000000000007E1";
        const defaultRowVersionBase64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

        // RowVersion – 0x... hex → base64 (mevcut kayıt düzenleniyorsa, yoksa sabit)
        let rowVersionBase64 = defaultRowVersionBase64;
        const $rowVerHidden = $("#hdnRowVersion");
        if ($rowVerHidden.length) {
            const hexVal = ($rowVerHidden.val() || "").trim();
            if (hexVal) {
                rowVersionBase64 = rowVersionHexToBase64(hexVal);
            }
        }

        // Tüm BaseEntity türevleri için ortak doldurucu
        const baseFor = (rvBase64) => buildBaseEntity(
            currentUserId,
            nowIso,
            rvBase64 || defaultRowVersionBase64 // null/falsy gelirse bile default RowVersion bas
        );

        // Fatura no ve tarih
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

        // Para birimi ve toplam
        const currency = (h.DocumentCurrencyCode || $("#ddlParaBirimi").val() || "TRY").toString().toUpperCase();

        let total = h.PayableAmount || 0;
        if (!total) {
            const txtTotal =
                $("#odenecekToplam").val() ||
                $("#odenecekToplam").text() ||
                "0";
            total = Utils.parseNumber(txtTotal);
        }

        // -------- ROOT INVOICE --------
        const invoice = {
            // BaseEntity (RowVersion kesin dolu)
            ...baseFor(rowVersionBase64),

            // Invoice alanları
            Id: 0,
            CustomerId: customerId || 0,
            InvoiceNo: invoiceNo,
            InvoiceDate: invoiceDateIso,
            Total: total,
            Currency: currency,

            // Ek alanlar (C# Invoice'ta olmasa bile JSON sorun değil)
            Ettn: h.Invoice_ID || null,
            BranchCode: $("#ddlSubeKodu").val() || null,
            SourceUrn: $("#ddlSourceUrn").val() || null,
            InvoicePrefix: $("#ddlInvoicePrefix").val() || null,
            TemplateName: $("#ddlGoruntuDosyasi").val() || null,
            InvoiceTypeCode: h.InvoiceTypeCode || null,
            Scenario: h.Scenario || null,

            // Navigation
            Customer: null,
            InvoicesItems: [],
            InvoicesTaxes: [],
            InvoicesDiscounts: [],
            Tourists: [],
            SgkRecords: [],
            ServicesProviders: [],
            Returns: [],
            InvoicesPayments: [],

            // Ek alanlar / irsaliye / kamu
            Despatchs: [],
            AdditionalFields: null,
            KamuAlicisi: model.kamuAlicisi || null
        };

        // -------- CUSTOMER --------
        const flatCustomer = model.Customer || readCustomerFromUI();
        const nameParts = splitNameSurname(flatCustomer);

        const customerEntity = {
            ...baseFor(), // RowVersion sabit dolu
            Id: customerId || 0,
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

        // Tek adres
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

        // -------- KALEMLER (InvoicesItem + Item + Brand + Unit) --------
        (model.invoiceLines || []).forEach((l, idx) => {
            const itemEntity = {
                ...baseFor(),
                Id: 0,
                Name: l.ItemName || ("Satır " + (idx + 1)),
                Code: l.SellerItemCode || ("ITEM-" + (idx + 1)),
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

            const lineTotal = Number(
                l.LineTotal ||
                (l.Amount || 0) + (l.KdvTutar || 0)
            );

            const invoicesItemEntity = {
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                ItemId: 0,
                Quantity: Number(l.Quantity || 0),
                Price: Number(l.Price || 0),
                Total: lineTotal,
                Invoice: null,
                Item: itemEntity
            };

            invoice.InvoicesItems.push(invoicesItemEntity);
        });

        // -------- VERGİLER (InvoicesTax) --------
        const taxMap = {};
        (model.invoiceLines || []).forEach(l => {
            const rate = Number(l.KdvOran || 0);
            const amount = Number(l.KdvTutar || 0);
            if (!taxMap[rate]) taxMap[rate] = 0;
            taxMap[rate] += amount;
        });

        Object.keys(taxMap).forEach(rateStr => {
            const rate = Number(rateStr);
            const amount = taxMap[rateStr] || 0;
            invoice.InvoicesTaxes.push({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                Name: "KDV",
                Rate: rate,
                Amount: amount,
                Invoice: null
            });
        });

        // -------- TOPLAM İSKONTO (InvoicesDiscount) --------
        let totalDiscount = 0;
        (model.invoiceLines || []).forEach(l => {
            totalDiscount += Number(l.DiscountAmount || 0);
        });
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

        // -------- SGK (SgkRecords) --------
        if (model.sgk) {
            const startDateIso = model.sgk.GonderimTarihiBaslangic
                ? new Date(model.sgk.GonderimTarihiBaslangic).toISOString()
                : nowIso;
            const endDateIso = model.sgk.GonderimTarihiBitis
                ? new Date(model.sgk.GonderimTarihiBitis).toISOString()
                : nowIso;

            invoice.SgkRecords.push({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                Type: model.sgk.IlaveFaturaTipi || "",
                Code: model.sgk.MukellefKodu || "",
                Name: model.sgk.MukellefAdi || "",
                No: model.sgk.DosyaNo || "",
                StartDate: startDateIso,
                EndDate: endDateIso,
                Invoice: null
            });
        }

        // -------- HİZMET SAĞLAYICI (ServicesProvider) --------
        const systemUser =
            ($("#hdnUserName").val() || $("#lblUserName").text() || "").trim() || "WEBUI";

        invoice.ServicesProviders.push({
            ...baseFor(),
            Id: 0,
            No: "SRV-" + Date.now(),
            SystemUser: systemUser,
            InvoiceId: 0,
            Invoice: null
        });

        // -------- ÖDEME (Payment + PaymentType + PaymentAccount + Bank + InvoicesPayment) --------
        if (model.payment) {
            const p = model.payment;

            const paymentTypeName =
                $("#PaymentMeansCode option:selected").text() ||
                p.PaymentMeansCode ||
                "Ödeme";

            const paymentChannelText =
                $("#PaymentChannelCode option:selected").text() ||
                p.PaymentChannelCode ||
                "";

            const paymentCurrency = (p.PayeeFinancialCurrencyCode || currency || "TRY").toString().toUpperCase();

            const paymentDateIso = p.PaymentDueDate
                ? p.PaymentDueDate + "T00:00:00"
                : invoice.InvoiceDate || nowIso;

            const paymentType = {
                ...baseFor(),
                Id: 0,
                Name: paymentTypeName,
                Desc: paymentChannelText || "Ödeme Şekli",
                Payments: []
            };

            const bankName = $("#txtBankName").val() || $("#txtBankName").text() || "";
            const bankCountry = $("#txtBankCountry").val() || flatCustomer.CountryName || "TR";
            const bankCity = $("#txtBankCity").val() || flatCustomer.CityName || "";
            const bankSwift = $("#txtSwiftCode").val() || "";

            const bank = {
                ...baseFor(),
                Id: 0,
                Name: bankName || "Banka",
                SwiftCode: bankSwift,
                Country: bankCountry,
                City: bankCity,
                PaymentAccounts: []
            };

            const accountName = p.PayeeFinancialAccount || "Hesap";
            const accountNo = $("#txtAccountNo").val() || p.PayeeFinancialAccount || "";
            const iban = $("#txtIban").val() || "";

            const paymentAccount = {
                ...baseFor(),
                Id: 0,
                Name: accountName,
                Desc: paymentChannelText || "",
                BankId: 0,
                AccountNo: accountNo,
                Iban: iban,
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

            const invoicePayment = {
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                PaymentId: 0,
                Invoice: null,
                Payment: paymentEntity
            };

            invoice.InvoicesPayments.push(invoicePayment);
        }

        // -------- İRSALİYE (Despatchs) --------
        if ((model.irsaliyeler || []).length) {
            invoice.Despatchs = model.irsaliyeler.map(d => ({
                ...baseFor(),
                Id: 0,
                InvoiceId: 0,
                DespatchNo: d.IrsaliyeNo,
                DespatchDate: d.IrsaliyeTarihi || null
            }));
        }

        // -------- Ek Alanlar (AdditionalFields) --------
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

        if (hasAdditional) {
            invoice.AdditionalFields = additional;
        }

        return invoice;
    }


    /***********************************************************
     *  SGK EK ALANLAR
     ***********************************************************/
    const sgkConfig = {
        "SAGLIK_ECZ": {
            mukellefKodu: "SGK Kurum Kodu",
            mukellefAdi: "SGK Kurum Adı",
            dosyaNo: "Reçete / Takip No",
            showSendingDate: true
        },
        "SAGLIK_HAS": {
            mukellefKodu: "SGK Kurum Kodu",
            mukellefAdi: "SGK Kurum Adı",
            dosyaNo: "Takip No",
            showSendingDate: true
        },
        "SAGLIK_OPT": {
            mukellefKodu: "SGK Kurum Kodu",
            mukellefAdi: "SGK Kurum Adı",
            dosyaNo: "Reçete No",
            showSendingDate: true
        },
        "SAGLIK_MED": {
            mukellefKodu: "SGK Kurum Kodu",
            mukellefAdi: "SGK Kurum Adı",
            dosyaNo: "Evrak No",
            showSendingDate: true
        },
        "ABONELIK": {
            mukellefKodu: "Abone No",
            mukellefAdi: "Abone Adı",
            dosyaNo: "Sözleşme / Müşteri No",
            showSendingDate: true
        },
        "MAL_HIZMET": {
            mukellefKodu: "Müşteri / Kurum Kodu",
            mukellefAdi: "Müşteri / Kurum Adı",
            dosyaNo: "Evrak / Dosya No",
            showSendingDate: true
        },
        "DIGER": {
            mukellefKodu: "Referans Kodu",
            mukellefAdi: "Referans Adı",
            dosyaNo: "Referans No",
            showSendingDate: false
        }
    };

    function updateSgkFields() {
        const val = $("#ddlilaveFaturaTipi").val();
        const cfg = sgkConfig[val];

        const $fields = $(".sgk-field");
        if (!val) {
            $fields.addClass("d-none");
            return;
        }

        $("#fieldMukellefKodu, #fieldMukellefAdi, #fieldDosyaNo, #fieldGonderimTarihi").removeClass("d-none");

        if (cfg) {
            $("#lblMukellefKodu").text(cfg.mukellefKodu);
            $("#lblMukellefAdi").text(cfg.mukellefAdi);
            $("#lblDosyaNo").text(cfg.dosyaNo);
            if (cfg.showSendingDate) {
                $("#fieldGonderimTarihi").removeClass("d-none");
            } else {
                $("#fieldGonderimTarihi").addClass("d-none");
            }
        } else {
            $("#lblMukellefKodu").text("Mükellef Kodu");
            $("#lblMukellefAdi").text("Mükellef Adı");
            $("#lblDosyaNo").text("Dosya / Evrak No");
            $("#fieldGonderimTarihi").removeClass("d-none");
        }
    }

    /***********************************************************
     *  DİĞER TABLOLAR - Yeni Satır Ekle / Sil
     ***********************************************************/
    function addNewIrsaliyeRow() {
        const row = `
        <tr>
            <td><input type="text" class="form-control input-sm irs-no" placeholder="İrsaliye No"></td>
            <td><input type="date" class="form-control input-sm irs-date"></td>
            <td class="text-center">
                <button type="button" class="btn btn-danger btn-sm js-irsaliye-remove">
                    <i class="fa fa-trash"></i>
                </button>
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
                <button type="button" class="btn btn-danger btn-sm js-satici-ek-remove">
                    <i class="fa fa-trash"></i>
                </button>
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
                <button type="button" class="btn btn-danger btn-sm js-satici-sube-ek-remove">
                    <i class="fa fa-trash"></i>
                </button>
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
                <button type="button" class="btn btn-danger btn-sm js-alici-ek-remove">
                    <i class="fa fa-trash"></i>
                </button>
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
                <button type="button" class="btn btn-danger btn-sm js-ekalani-remove">
                    <i class="fa fa-trash"></i>
                </button>
            </td>
        </tr>`;
        $("#tbodyEkalani").append(row);
    }

    /***********************************************************
     *  ZORUNLU ALAN KONTROLÜ
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

        // HEADER
        requireSelect("#ddlSubeKodu", "Şube seçiniz.");
        requireSelect("#ddlSourceUrn", "Gönderici etiketini seçiniz.", [""]);
        requireSelect("#ddlInvoicePrefix", "Fatura ön ekini seçiniz.", [""]);
        requireSelect("#ddlGoruntuDosyasi", "Görüntü dosyasını seçiniz.", [""]);
        requireInput("#txtUUID", "ETTN üretilemedi.");
        requireInput("#txtIssueDate", "Fatura tarihini giriniz.");
        requireInput("#txtIssueTime", "Fatura saatini giriniz.");
        requireSelect("#ddlParaBirimi", "Para birimini seçiniz.", [""]);

        // ALICI
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

        // ÖDEME
        requireSelect("#PaymentMeansCode", "Ödeme şeklini seçiniz.", [""]);
        requireInput("#PaymentDueDate", "Ödeme tarihini giriniz.");
        requireSelect("#PayeeFinancialCurrencyCode", "Hesap para birimini seçiniz.", [""]);

        // KALEMLER
        if ($("#invoiceBody tr").length === 0) {
            errors.push("En az bir fatura kalemi ekleyiniz.");
        } else {
            let hasNamedLine = false;
            $("#invoiceBody tr").each(function () {
                const name = ($(this).find(".js-line-name").val() || "").trim();
                const qty = Utils.parseNumber($(this).find(".js-line-qty").val());
                if (name && qty > 0) {
                    hasNamedLine = true;
                }
            });
            if (!hasNamedLine) {
                errors.push("En az bir satırda mal/hizmet adı ve miktar giriniz.");
            }
        }

        // KAMU SENARYO
        const isKamuSenaryo = $("#ddlSenaryo").val() === "KAMU";
        if (isKamuSenaryo) {
            requireInput("#txtKamuVkn", "Kamu alıcı VKN giriniz.");
            requireInput("#txtKamuUnvan", "Kamu alıcı unvanını giriniz.");
            requireInput("#txtKamuUlke", "Kamu alıcı ülke bilgisini giriniz.");
            requireInput("#txtKamuIl", "Kamu alıcı il bilgisini giriniz.");
        }

        if (errors.length && invalidElements.length) {
            invalidElements[0].focus();
        }

        return {
            isValid: errors.length === 0,
            errors: errors
        };
    }

    /***********************************************************
     *  TASLAK KAYIT – createInvoice çağrısı (Invoice.cs graph)
     ***********************************************************/
    async function saveInvoiceDraft(entity) {
        try {
            // Taslak bilgisi – C# entity'de yok, ama ekstra alan JSON'da sorun olmaz
            entity.IsDraft = true;
            const result = await createInvoice(entity);
            return result;
        } catch (err) {
            console.error('[Invoice] Taslak kaydedilirken hata:', err);
            showAlert('danger', 'Taslak fatura kaydedilirken bir hata oluştu.');
            throw err;
        }
    }

    /***********************************************************
     *  EVENT BINDINGS
     ***********************************************************/
    function bindEvents() {
        // Manuel şehir
        $(document).on("change", "#chkManuelSehir", toggleManuelSehir);

        // Alıcıdan kamuya kopyala
        $(document).on("click", "#btnAliciBilgilerindenKopyala", function (e) {
            e.preventDefault();
            copyAliciToKamu();
        });

        // Müşteri seçimi (demo)
        $(document).on("change", "#ddlMusteriAra", function () {
            const id = parseInt($(this).val(), 10);
            const cust = TestCustomers.find(x => x.id === id);
            fillCustomerFieldsFromTest(cust);
        });

        // Header değişiklikleri
        bindHeaderInputs();

        // Fatura kalemleri
        $(document).on("click", "#btnLineAdd", function () {
            addNewLineRow();
        });

        $(document).on("click", ".js-line-remove", function () {
            $(this).closest("tr").remove();
            renumberLines();
            recalcTotals();
        });

        $(document).on("keyup change",
            "#invoiceBody .js-line-qty, " +
            "#invoiceBody .js-line-price, " +
            "#invoiceBody .js-line-disc-rate, " +
            "#invoiceBody .js-line-kdv",
            function () {
                const $row = $(this).closest("tr");
                calcLine($row);
            });

        // KDV hariç/dahil
        $(document).on("change", "#kdvStatu", function () {
            $("#invoiceBody tr").each(function () {
                calcLine($(this));
            });
        });

        // Önizleme
        $(document).on("click", "#btnPreviewInvoice", function (e) {
            e.preventDefault();
            recalcTotals();
            const model = buildInvoiceModelFromUI();
            renderInvoicePreview(model);
            $("#invoicePreviewModal").modal("show");
        });

        
        // TASLAK / GİB GÖNDER → Invoice.cs + tüm ilişkiler dolu create API
        $(document).on("click", "#btnDraftSave, #btnSendToGib", async function (e) {
            e.preventDefault();

            const isDraft = this.id === "btnDraftSave";

            // Taslak Kaydet daha önce başarılı olduysa ve buton disabled ise 2. kez çalışmasın
            if (isDraft && $("#btnDraftSave").prop("disabled")) {
                return;
            }

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
                // 1) SADECE TASLAK KAYDET
                if (isDraft) {
                    const res = await saveInvoiceDraft(entity);
                    console.log("Taslak olarak kaydedilen Invoice:", res);
                    showAlert("success", "Taslak fatura başarıyla kaydedildi.");

                    // Taslak kayıttan dönen invoice Id'yi al (response schema'na göre burayı düzenle)
                    const invoiceId = res && res.id;

                    if (invoiceId) {
                        // Hidden input'a yaz
                        $("#hfInvoiceId").val(invoiceId);

                        // Aynı sayfada ikinci kez taslak kaydetmeyi engelle
                        $("#btnDraftSave").prop("disabled", true);
                    } else {
                        console.warn("Taslak kayıttan invoiceId alınamadı. Response:", res);
                    }

                    return;
                }

                // 2) GİB GÖNDER BUTONU

                let invoiceIdFromHidden = $("#hfInvoiceId").val();
                const draftButtonDisabled = $("#btnDraftSave").prop("disabled");

                // a) Taslak daha hiç kaydedilmemişse VEYA hidden boş ise
                //    önce taslağı kaydet, sonra GİB'e gönder
                if (!invoiceIdFromHidden || !draftButtonDisabled) {
                    const draftRes = await saveInvoiceDraft(entity);
                    console.log("GİB öncesi otomatik taslak kaydedildi:", draftRes);

                    const newInvoiceId =
                        draftRes?.invoiceId ||
                        draftRes?.id ||
                        (draftRes?.data && (draftRes.data.invoiceId || draftRes.data.id));

                    if (!newInvoiceId) {
                        showAlert("danger", "Taslak fatura kaydedildi, ancak Invoice Id alınamadı. GİB gönderilemedi.");
                        return;
                    }

                    $("#hfInvoiceId").val(newInvoiceId);
                    $("#btnDraftSave").prop("disabled", true);

                    invoiceIdFromHidden = newInvoiceId;
                }

                // b) Elimizde artık mutlaka bir Invoice Id var → GİB Portal API'ye gönder
                // DİKKAT: Burada eskiden createInvoice(entity) çağırıyordun; artık GİB API kullanıyoruz.
                const gibRes = await sendInvoiceToGibById(invoiceIdFromHidden);
                console.log("GİB'e gönderim sonucu:", gibRes);

                // GİB response alanlarını kendi API'ne göre çek
                const statusCode =
                    gibRes?.statusCode || gibRes?.StatusCode || gibRes?.code;
                const uuid = gibRes && gibRes.id;                   // PDF URL'de kullanılacak
                const invoiceNumber = gibRes && gibRes.invoiceNumber;
                const documentId =
                    gibRes?.documentId || gibRes?.DocumentId || gibRes?.id;

                // 3) GİB dönüşünü hidden input'lara bas → Download butonları bunları kullanacak
                $("#hfGibStatusCode").val(statusCode || "");
                $("#hfGibInvoiceUuid").val(uuid || "");
                $("#hfGibInvoiceNumber").val(invoiceNumber || "");
                $("#hfGibDocumentId").val(documentId || "");

                if (hasUuid && hasInvoiceNumber) {
                    showAlert("success", "Fatura başarıyla GİB'e gönderildi.");

                    // PDF / XML butonlarını aktif et
                    $("#btnDownloadPDF").prop("disabled", false);
                    $("#btnDownloadXML").prop("disabled", false);
                } else {
                    console.warn("GİB yanıtında uuid veya invoiceNumber eksik:", gibRes);

                    showAlert(
                        "danger",
                        (gibRes?.message || gibRes?.Message) ||
                        "Fatura GİB'e gönderilirken bir hata oluştu (uuid veya fatura numarası alınamadı)."
                    );
                }

            } catch (err) {
                console.error("Fatura kaydedilirken/gönderilirken hata:", err);
                showAlert(
                    "danger",
                    isDraft
                        ? "Taslak fatura kaydedilirken bir hata oluştu."
                        : "Fatura GİB'e gönderilirken bir hata oluştu."
                );
            }
        });

        // PDF İndir
        $(document).on("click", "#btnDownloadPDF", function (e) {
            e.preventDefault();

            // GİB'den dönen uuid (response.id)
            const uuid = $("#hfGibInvoiceUuid").val();

            if (!uuid) {
                showAlert("danger", "Önce faturayı GİB'e başarıyla göndermelisiniz.");
                return;
            }

            const baseUrl = window.gibPortalApiBaseUrl || window.gibPortalApiBaseUrl;
            if (!baseUrl) {
                showAlert("danger", "GİB Portal API adresi tanımlı değil (gibPortalApiBaseUrl).");
                return;
            }

            // baseUrl: https://localhost:7151/api/  (appsettings’ten geliyor)
            const normBase = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";

            // Hedef URL:
            // https://localhost:7151/api/TurkcellEFatura/einvoice/outbox/pdf/{uuid}?standardXslt=true
            const url =
                normBase +
                "TurkcellEFatura/einvoice/outbox/pdf/" +
                encodeURIComponent(uuid) +
                "?standardXslt=true";

            console.log("[GIB] PDF download URL:", url);

            // PDF endpoint'ine top-level navigation ile gidiyoruz, CORS derdi yok
            window.open(url, "_blank");
        });

        // XML (UBL) İndir
        $(document).on("click", "#btnDownloadXML", function (e) {
            e.preventDefault();

            // GİB gönderim cevabından gelen uuid (response.id)
            const uuid = $("#hfGibInvoiceUuid").val();

            if (!uuid) {
                showAlert("danger", "Önce faturayı GİB'e başarıyla göndermelisiniz.");
                return;
            }

            const baseUrl = window.gibPortalApiBaseUrl || window.gibPortalApiBaseUrl;
            if (!baseUrl) {
                showAlert("danger", "GİB Portal API adresi tanımlı değil (gibPortalApiBaseUrl).");
                return;
            }

            // baseUrl: https://localhost:7151/api/  (appsettings’ten geliyor)
            const normBase = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";

            // Hedef URL:
            // https://localhost:7151/api/TurkcellEFatura/einvoice/outbox/ubl/{uuid}?standardXslt=true
            const url =
                normBase +
                "TurkcellEFatura/einvoice/outbox/ubl/" +
                encodeURIComponent(uuid) +
                "?standardXslt=true";

            console.log("[GIB] UBL download URL:", url);

            // XML/UBL dosyasını indirmek için yeni sekmede aç
            window.open(url, "_blank");
        });


        // SGK ilave fatura tipi
        $(document).on("change", "#ddlilaveFaturaTipi", updateSgkFields);

        // İrsaliye
        $(document).on("click", "#btnYeniIrsaliyeEkle", function () {
            addNewIrsaliyeRow();
        });
        $(document).on("click", ".js-irsaliye-remove", function () {
            $(this).closest("tr").remove();
        });

        // Satıcı ek alanlar
        $(document).on("click", "#btnAddSaticiEk", function () {
            addNewSaticiEkRow();
        });
        $(document).on("click", ".js-satici-ek-remove", function () {
            $(this).closest("tr").remove();
        });

        // Satıcı şube ek alanlar
        $(document).on("click", "#btnAddSaticiSubeEk", function () {
            addNewSaticiSubeEkRow();
        });
        $(document).on("click", ".js-satici-sube-ek-remove", function () {
            $(this).closest("tr").remove();
        });

        // Alıcı ek alanlar
        $(document).on("click", "#btnAddAliciEk", function () {
            addNewAliciEkRow();
        });
        $(document).on("click", ".js-alici-ek-remove", function () {
            $(this).closest("tr").remove();
        });

        // Ek alanlar
        $(document).on("click", "#btnAddEkalani", function () {
            addNewEkalaniRow();
        });
        $(document).on("click", ".js-ekalani-remove", function () {
            $(this).closest("tr").remove();
        });

        // Excel / Medula / Cezaevi / Toplu işlemler - placeholder
        $(document).on("click", "#excelLineModalOpen", function (e) {
            e.preventDefault();
            console.log("Excel'den yükleme burada implemente edilecek.");
        });
        $(document).on("click", "#excelLineExport", function (e) {
            e.preventDefault();
            console.log("Excel'e aktarma burada implemente edilecek.");
        });
        $(document).on("click", "#btnMedulaAktar", function () {
            console.log("Medula aktarım fonksiyonu burada implemente edilebilir.");
        });
        $(document).on("click", "#btnCezaeviAktar", function () {
            console.log("Cezaevi aktarım fonksiyonu burada implemente edilebilir.");
        });
        $(document).on("click", "#btnTopluIndirim", function () {
            console.log("Toplu indirim/Artırım fonksiyonu burada implemente edilebilir.");
        });
        $(document).on("click", "#btnTopluVergi", function () {
            console.log("Toplu vergi fonksiyonu burada implemente edilebilir.");
        });
    }

    /***********************************************************
     *  INIT
     ***********************************************************/
    function initDefaults() {
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

        // Müşteri dropdown
        fillMusteriAraDropdown();

        // ListData dropdownlar
        fillSubeDropdown();
        fillSourceUrnDropdown();
        fillInvoicePrefixDropdown();
        fillGoruntuDosyasiDropdown();
        fillAliciEtiketiDropdown();
        fillParaBirimiDropdown();
        fillPaymentDropdowns();

        if ($("#txtKurBilgisi").length && !$("#txtKurBilgisi").val()) {
            $("#txtKurBilgisi").val("1");
        }

        if ($("#invoicePreviewModal .modal-dialog").length) {
            $("#invoicePreviewModal .modal-dialog").css({
                "max-width": "95%",
                "width": "95%"
            });
        }
        if ($("#invoicePreviewContent").length) {
            $("#invoicePreviewContent").css({
                "max-height": "80vh",
                "overflow-x": "auto",
                "overflow-y": "auto"
            });
        }

        updateSgkFields();

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

        if ($("#btnLineAdd").length) {
            $("#btnLineAdd").trigger("click");
        }
    }

    function init() {
        bindEvents();
        initDefaults();
    }

    const InvoiceApp = {
        init,
        recalcTotals,
        buildInvoiceModelFromUI,
        buildInvoiceEntityFromUI
    };

    global.InvoiceApp = InvoiceApp;

})(window, jQuery);

// Sayfa yüklenince başlat
$(function () {
    if (window.InvoiceApp && typeof window.InvoiceApp.init === "function") {
        window.InvoiceApp.init();
    }
});
