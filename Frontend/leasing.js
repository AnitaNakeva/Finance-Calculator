// if (![price, downPayment, months].every(isValidNumber) || price <= 0 || months <= 0 || downPayment < 0) {
//   showError("leaseResult", "Попълни коректни стойности.");
//   return;
// }
//
// if (downPayment > price) {
//   showError("leaseResult", "Първоначалната вноска не може да е по-голяма от цената.");
//   return;
// }
//
// const data = { price, downPayment, months };
async function calculateLeasing() {
  const itemPrice = Number(document.getElementById("ItemPrice").value);
  const downPayment = Number(document.getElementById("DownPayment").value);
  const termMonths = Number(document.getElementById("TermMonths").value);
  const monthlyPayment = Number(document.getElementById("MonthlyPayment").value);
  const processingFeePercent = Number(document.getElementById("ProcessingFeePercent").value);


const data ={
  itemPrice:itemPrice,
    downPayment:downPayment,
    termMonths:termMonths,
    monthlyPayment:monthlyPayment,
    processingFeePercent:processingFeePercent,
  };

  function renderLeasingResult(r) {
    document.getElementById("leasingResultBox").style.display = "block";

    leaseFinanced.textContent = r.financedAmount.toFixed(2) + " лв.";
    leaseFees.textContent = r.processingFeeAmount.toFixed(2) + " лв.";
    leaseTotalPaid.textContent = r.totalPaid.toFixed(2) + " лв.";

    leaseOverAmount.textContent =
      r.overpaymentAmount.toFixed(2) + " лв.";

    leaseOverPercent.textContent =
      r.overpaymentPercent.toFixed(2) + " %";

    leaseOverAmount.className =
      r.overpaymentAmount >= 0 ? "positive" : "negative";

    leaseOverPercent.className =
      r.overpaymentPercent >= 0 ? "positive" : "negative";
  }

  try {
    const result = await postLeasing(data);
    renderLeasingResult(result);
  } catch (err) {
    showError("leaseResult", err.message);
  }
}
