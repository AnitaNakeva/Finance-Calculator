


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
    document.getElementById("refResult").textContent =
      JSON.stringify(result, null, 2);
  } catch (err) {
    showError("refResult", err.message);
  }
}


