async function calculateCredit() {
  const principal = Number(document.getElementById("Principal").value);
  const termMonths = Number(document.getElementById("TermMonths1").value);
  const annualInterestRate = Number(document.getElementById("AnnualInterestRate").value);
  const paymentType = document.getElementById("PaymentType").value;


  if (!paymentType) {
    showError("creditResult", "Моля, изберете тип на вноските.");
    return;
  }
  const data = {
    principal: principal,
    termMonths: termMonths,
    annualInterestRate: annualInterestRate,
    paymentType: paymentType, // "Annuity" или "Decreasing"

    graceMonths: Number(document.getElementById("GraceMonths").value || 0),
    promoMonths: Number(document.getElementById("PromoMonths").value || 0),
    promoAnnualInterestRate: Number(document.getElementById("PromoAnnualInterestRate").value || 0)
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

  document.getElementById("creditMonthly").textContent =
    result.monthlyPayment.toFixed(2) + " лв.";

  document.getElementById("creditInterest").textContent =
    result.totalInterest.toFixed(2) + " лв.";

  document.getElementById("creditTotal").textContent =
    result.totalPaid.toFixed(2) + " лв.";
}

