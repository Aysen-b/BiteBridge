let cart = JSON.parse(localStorage.getItem("cart")) || [];

function addToCart(item) {
    cart.push(item);
    localStorage.setItem("cart", JSON.stringify(cart));
    alert("Added to cart!");
    displayCart();
}

function addMenuItemToCart(button) {
    const menuItemId = button.dataset.id;
    const itemName = button.dataset.name;
    const basePrice = parseFloat(button.dataset.price);

    const selectedOptions = [];
    let extraTotal = 0;

    const optionCheckboxes = document.querySelectorAll(".option-checkbox-" + menuItemId);

    optionCheckboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            const optionName = checkbox.dataset.name;
            const optionType = checkbox.dataset.type;
            const optionPrice = parseFloat(checkbox.dataset.price || "0");

            selectedOptions.push({
                name: optionName,
                type: optionType,
                price: optionPrice
            });

            extraTotal += optionPrice;
        }
    });

    let finalName = itemName;

    if (selectedOptions.length > 0) {
        const optionNames = selectedOptions.map(option => option.name).join(", ");
        finalName = itemName + " (" + optionNames + ")";
    }

    const finalPrice = basePrice + extraTotal;

    addToCart({
        name: finalName,
        price: finalPrice,
        basePrice: basePrice,
        options: selectedOptions
    });
}

function getCart() {
    return JSON.parse(localStorage.getItem("cart")) || [];
}

function clearCart() {
    localStorage.removeItem("cart");
    cart = [];
    hidePayment();
}

function removeFromCart(index) {
    cart.splice(index, 1);
    localStorage.setItem("cart", JSON.stringify(cart));
    displayCart();
    hidePayment();
}

function calculateTotal(cartItems) {
    let total = 0;
    cartItems.forEach(item => total += Number(item.price));
    return total;
}

function showPayment() {
    const cartItems = getCart();

    if (cartItems.length === 0) {
        alert("Cart is empty!");
        return;
    }

    const paymentSection = document.getElementById("payment-section");
    const paymentSummary = document.getElementById("payment-summary");

    let total = calculateTotal(cartItems);

    let summaryHtml = "<h4>Order Summary</h4><ul>";

    cartItems.forEach(item => {
        summaryHtml += `<li>${item.name} → ${item.price} TL</li>`;
    });

    summaryHtml += `</ul><p><b>Total:</b> ${total} TL</p>`;

    paymentSummary.innerHTML = summaryHtml;
    paymentSection.style.display = "block";
}

function hidePayment() {
    const paymentSection = document.getElementById("payment-section");

    if (paymentSection) {
        paymentSection.style.display = "none";
    }
}

function validatePaymentForm() {
    const cardName = document.getElementById("card-name").value.trim();
    const cardNumber = document.getElementById("card-number").value.trim();
    const cardExpiry = document.getElementById("card-expiry").value.trim();
    const cardCvv = document.getElementById("card-cvv").value.trim();

    if (cardName.length < 3) {
        alert("Please enter the card holder name.");
        return false;
    }

    if (!/^[0-9]{16}$/.test(cardNumber)) {
        alert("Card number must be 16 digits.");
        return false;
    }

    if (!/^[0-9]{2}\/[0-9]{2}$/.test(cardExpiry)) {
        alert("Expiry date must be in MM/YY format.");
        return false;
    }

    if (!/^[0-9]{3}$/.test(cardCvv)) {
        alert("CVV must be 3 digits.");
        return false;
    }

    return true;
}

function checkout() {
    const cartItems = getCart();

    if (cartItems.length === 0) {
        alert("Cart is empty!");
        return;
    }

    if (!validatePaymentForm()) {
        return;
    }

    let total = calculateTotal(cartItems);

    fetch("/Order/Checkout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            items: JSON.stringify(cartItems),
            totalPrice: total
        })
    })
    .then(response => {
        if (response.status === 401) {
            alert("Please login before payment.");
            return;
        }

        if (response.ok) {
            alert("Payment successful! Your order has been created.");
            clearCart();
            displayCart();

            window.location.href = "/Order/List";
        } else {
            alert("Payment failed!");
        }
    })
    .catch(() => {
        alert("Payment failed!");
    });
}

function displayCart() {
    const cartItems = getCart();
    const cartContainer = document.getElementById("cart-items");
    const totalElement = document.getElementById("total-price");

    if (!cartContainer) {
        return;
    }

    cartContainer.innerHTML = "";

    let total = 0;

    cartItems.forEach(function (item, index) {
        total += Number(item.price);

        cartContainer.innerHTML += `
            <p>
                ${item.name} - ${item.price} TL
                <button onclick="removeFromCart(${index})">Remove</button>
            </p>
        `;
    });

    if (totalElement) {
        totalElement.innerHTML = "Total: " + total + " TL";
    }
}

window.onload = function () {
    displayCart();
};