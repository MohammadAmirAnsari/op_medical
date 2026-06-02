window.openBankPayment = (paymentUrl, encRequest, accessCode) => {
    const form = document.createElement("form");
    form.method = "POST";
    form.action = paymentUrl;
    form.target = "_blank";

    const enc = document.createElement("input");
    enc.type = "hidden";
    enc.name = "encRequest";
    enc.value = encRequest;

    const access = document.createElement("input");
    access.type = "hidden";
    access.name = "access_code";
    access.value = accessCode;

    form.appendChild(enc);
    form.appendChild(access);

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
};

window.printHtml = (elementId) => {
    printJS({
        printable: elementId,
        type: 'html',
        targetStyles: ['*'],
        scanStyles: true,
        css: [
            '../../_content/MudBlazor/MudBlazor.min.css'
        ]
    });
}