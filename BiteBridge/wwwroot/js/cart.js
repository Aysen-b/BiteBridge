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
}
function displayCart() {
    const cartItems = getCart();
    const cartContainer = document.getElementById("cart-items");

    if (!cartContainer) {
        return;
    }

    cartContainer.innerHTML = "";

    cartItems.forEach(function (item) {
        cartContainer.innerHTML += `
            <p>${item.name} - ${item.price} TL</p>
        `;
    });
}
window.onload = function () {
    displayCart();
};