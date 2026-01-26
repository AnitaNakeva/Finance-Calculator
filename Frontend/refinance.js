


// async function calculate() {
//  const amount = Number(document.getElementById("refAmount").value);
//   const interest = Number(document.getElementById("refInterest").value);
//   const months = Number(document.getElementById("refMonths").value);
//
//   if (![amount, interest, months].every(isValidNumber) || amount <= 0 || months <= 0) {
//     showError("refResult", "Попълни коректни стойности.");
//     return;
//   }
//
//   const data = { amount, interest, months };
//
//   try {
//     const result = await postRefinance(data);
//     document.getElementById("refResult").textContent = JSON.stringify(result, null, 2);
//   } catch (err) {
//     showError("refResult", err.message);
//   }
//
//
// }


async function calculate() {
  const currentPrincipal = Number(document.getElementById("CurrentPrincipal").value);
  const currentAnnualInterestRate = Number(document.getElementById("CurrentAnnualInterestRate").value);
  const currentTermMonths = Number(document.getElementById("CurrentTermMonths").value);
  const paymentsMade = Number(document.getElementById("PaymentsMade").value);
  const earlyRepaymentFeePercent = Number(document.getElementById("EarlyRepaymentFeePercent").value);
  const newAnnualInterestRate = Number(document.getElementById("NewAnnualInterestRate").value);
  const upfrontFeesPercent = Number(document.getElementById("UpfrontFeesPercent").value);
  const upfrontFeesFixed = Number(document.getElementById("UpfrontFeesFixed").value);


  if (currentPrincipal <= 0 || currentTermMonths <= 0) {
    showError("refResult", "Попълни коректни стойности.");
    return;
  }

  const data = {
    currentPrincipal: currentPrincipal,
    currentTermMonths: currentTermMonths,
    currentAnnualInterestRate:currentAnnualInterestRate ,
    paymentsMade: paymentsMade,
    earlyRepaymentFeePercent: earlyRepaymentFeePercent,
    newAnnualInterestRate: newAnnualInterestRate,
    upfrontFeesPercent: upfrontFeesPercent,
    upfrontFeesFixed: upfrontFeesFixed
  };




  try {
    const result = await postRefinance(data);
    renderRefinanceResult(result, data);
  } catch (err) {
    showError("refResult", err.message);
  }
}
function renderRefinanceResult(result, request) {
  document.getElementById("refResultBox").style.display = "block";

  document.getElementById("curInterest").textContent =
    request.currentAnnualInterestRate.toFixed(2) + " %";

  document.getElementById("newInterest").textContent =
    request.newAnnualInterestRate.toFixed(2) + " %";

  document.getElementById("curTerm").textContent =
    request.currentTermMonths;

  document.getElementById("newTerm").textContent =
    result.remainingMonths;

  document.getElementById("curFees").textContent =
    result.earlyRepaymentFeeAmount.toFixed(2) + " лв.";

  document.getElementById("newFees").textContent =
    (result.upfrontFeesPercentAmount + result.upfrontFeesFixedAmount).toFixed(2) + " лв.";

  document.getElementById("curMonthly").textContent =
    result.currentMonthlyPayment.toFixed(2) + " лв.";

  document.getElementById("newMonthly").textContent =
    result.newMonthlyPayment.toFixed(2) + " лв.";

  const monthlySave = result.currentMonthlyPayment - result.newMonthlyPayment;
  const monthlyDiff = document.getElementById("monthlyDiff");
  monthlyDiff.textContent = monthlySave.toFixed(2) + " лв.";
  monthlyDiff.className = monthlySave >= 0 ? "positive" : "negative";

  document.getElementById("curTotal").textContent =
    result.currentTotalPaidRemaining.toFixed(2) + " лв.";

  document.getElementById("newTotal").textContent =
    result.newTotalPaid.toFixed(2) + " лв.";

  const totalDiff = document.getElementById("totalDiff");
  totalDiff.textContent = result.savings.toFixed(2) + " лв.";
  totalDiff.className = result.savings >= 0 ? "positive" : "negative";
}






