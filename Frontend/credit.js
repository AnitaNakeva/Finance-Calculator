async function calculateCredit() {
  const principal = Number(document.getElementById("Principal").value);
  const termMonths = Number(document.getElementById("TermMonths1").value);
  const annualInterestRate = Number(document.getElementById("AnnualInterestRate").value);
  const paymentType = document.getElementById("PaymentType").value;
  const applicationFee = Number(document.getElementById("ApplicationFee").value || 0);
  const processingFee = Number(document.getElementById("ProcessingFee").value || 0);
  const otherInitialFees = Number(document.getElementById("OtherInitialFees").value || 0);
  const monthlyManagementFee = Number(document.getElementById("MonthlyManagementFee").value || 0);
  const otherMonthlyFees = Number(document.getElementById("OtherMonthlyFees").value || 0);
  const annualManagementFee = Number(document.getElementById("AnnualManagementFee").value || 0);
  const otherAnnualFees = Number(document.getElementById("OtherAnnualFees").value || 0);

  if (!paymentType) {
    showError("creditResult", "Моля, изберете тип на погашението.");
    return;
  }

  const data = {
    principal: principal,
    termMonths: termMonths,
    annualInterestRate: annualInterestRate,
    paymentType: paymentType,
    graceMonths: Number(document.getElementById("GraceMonths").value || 0),
    promoMonths: Number(document.getElementById("PromoMonths").value || 0),
    promoAnnualInterestRate: Number(document.getElementById("PromoAnnualInterestRate").value || 0),
    applicationFee: applicationFee,
    processingFee: processingFee,
    otherInitialFees: otherInitialFees,
    monthlyManagementFee: monthlyManagementFee,
    otherMonthlyFees: otherMonthlyFees,
    annualManagementFee: annualManagementFee,
    otherAnnualFees: otherAnnualFees
  };

  try {
    const result = await postCredit(data);
    renderCreditResult(result);
  } catch (err) {
    showError("creditResult", err.message);
  }
}

function renderCreditResult(result) {
  document.getElementById("creditResultBox").style.display = "block";

  const toNumber = (value) =>
    typeof value === "number" && Number.isFinite(value) ? value : 0;
  const formatCurrency = (value) => toNumber(value).toFixed(2) + " лв.";
  const formatPercent = (value) => toNumber(value).toFixed(2) + " %";

  document.getElementById("creditMonthly").textContent =
    result.monthlyPayment.toFixed(2) + " лв.";

  document.getElementById("creditInterest").textContent =
    result.totalInterest.toFixed(2) + " лв.";

  document.getElementById("creditInitialFees").textContent =
    formatCurrency(result.initialFeesTotal);

  document.getElementById("creditMonthlyFees").textContent =
    formatCurrency(result.monthlyFeesTotal);

  document.getElementById("creditAnnualFees").textContent =
    formatCurrency(result.annualFeesTotal);

  document.getElementById("creditTotalFees").textContent =
    formatCurrency(result.totalFees);

  document.getElementById("creditTotal").textContent =
    result.totalPaid.toFixed(2) + " лв.";

  document.getElementById("creditApr").textContent =
    formatPercent(result.annualPercentageRate);
}
