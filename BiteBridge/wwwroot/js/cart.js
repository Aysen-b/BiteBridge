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
}
function removeFromCart(index) {
    cart.splice(index, 1);
    localStorage.setItem("cart", JSON.stringify(cart));
    displayCart();
}
function checkout() {
    const cartItems = getCart();

    let total = 0;
    cartItems.forEach(item => total += item.price);

    fetch("/Order/Checkout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            userEmail: "test@test.com",
            items: JSON.stringify(cartItems),
            totalPrice: total
        })
    })
    .then(response => {
        if (response.ok) {
            alert("Payment successful!");
            clearCart();
            displayCart();
        } else {
            alert("Payment failed!");
        }
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