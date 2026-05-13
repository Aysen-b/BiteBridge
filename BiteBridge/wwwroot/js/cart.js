let cart = JSON.parse(localStorage.getItem("cart")) || [];

function addToCart(item) {
    cart.push(item);
    localStorage.setItem("cart", JSON.stringify(cart));
    alert("Added to cart!");
    displayCart();
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
    cartItems.forEach(item => total += item.price);
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

    let groupedItems = {};

    cartItems.forEach(item => {
        if (!groupedItems[item.name]) {
            groupedItems[item.name] = {
                name: item.name,
                count: 0,
                total: 0
            };
        }

        groupedItems[item.name].count++;
        groupedItems[item.name].total += item.price;
    });

    let summaryHtml = "<h4>Order Summary</h4><ul>";

    Object.values(groupedItems).forEach(item => {
        summaryHtml += `<li>${item.name} x ${item.count} → ${item.total} TL</li>`;
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
        total += item.price;

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