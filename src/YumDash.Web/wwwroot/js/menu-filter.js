import React, { useEffect, useMemo, useState } from "https://esm.sh/react@18";
import { createRoot } from "https://esm.sh/react-dom@18/client";

const element = React.createElement;
const host = document.getElementById("menu-app");

function MenuApp({ categories, itemsUrl }) {
    const [items, setItems] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");
    const [filters, setFilters] = useState({
        category: "",
        maxPrice: "",
        allergen: "",
        excludeAllergens: true
    });

    useEffect(() => {
        let isMounted = true;

        async function loadItems() {
            try {
                setIsLoading(true);
                const response = await fetch(itemsUrl, {
                    headers: {
                        Accept: "application/json"
                    }
                });

                if (!response.ok) {
                    throw new Error(`Request failed with status ${response.status}`);
                }

                const data = await response.json();
                if (isMounted) {
                    setItems(data);
                    setError("");
                }
            } catch (requestError) {
                if (isMounted) {
                    setError("We couldn't load the menu right now.");
                }
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        }

        loadItems();

        return () => {
            isMounted = false;
        };
    }, [itemsUrl]);

    const filteredItems = useMemo(() => {
        const allergenTerms = filters.allergen
            .split(",")
            .map((term) => term.trim().toLowerCase())
            .filter((term) => term.length > 0);
        const maxPriceValue = filters.maxPrice === "" ? null : Number(filters.maxPrice);

        return items.filter((item) => {
            if (filters.category && item.category !== filters.category) {
                return false;
            }

            if (maxPriceValue !== null && !Number.isNaN(maxPriceValue) && item.price > maxPriceValue) {
                return false;
            }

            if (allergenTerms.length > 0) {
                const allergens = (item.allergens || "")
                    .split(",")
                    .map((term) => term.trim().toLowerCase())
                    .filter((term) => term.length > 0);
                const hasMatchingAllergen = allergenTerms.some((term) => allergens.includes(term));

                if (filters.excludeAllergens && hasMatchingAllergen) {
                    return false;
                }

                if (!filters.excludeAllergens && !hasMatchingAllergen) {
                    return false;
                }
            }

            return true;
        });
    }, [filters, items]);

    function updateFilter(name, value) {
        setFilters((current) => ({
            ...current,
            [name]: value
        }));
    }

    const groupedItems = useMemo(() => ({
        Appetizers: filteredItems.filter((item) => item.category === "Appetizers"),
        Entrees: filteredItems.filter((item) => item.category === "Entrees"),
        Desserts: filteredItems.filter((item) => item.category === "Desserts"),
        Beverages: filteredItems.filter((item) => item.category === "Beverages"),
        Specials: filteredItems.filter((item) => item.category === "Specials")
    }), [filteredItems]);

    function renderMenuSection(title, sectionItems) {
        return element(
            "section",
            { className: "menu-column", key: title },
            element("h2", { className: "menu-column-title" }, title),
            sectionItems.length
                ? sectionItems.map((item) =>
                    element(
                        "div",
                        { className: "menu-list-item", key: item.id },
                        element(
                            "div",
                            { className: "menu-list-row" },
                            element("strong", { className: "menu-item-name" }, item.name),
                            element("span", { className: "menu-item-price" }, `$${Number(item.price).toFixed(2)}`)
                        ),
                        element("p", { className: "menu-item-description" }, item.description),
                        element(
                            "small",
                            { className: "menu-item-allergens" },
                            item.allergens
                                ? `Allergens: ${item.allergens}`
                                : "Allergens: none listed"
                        )
                    ))
                : null
        );
    }

    const visibleSections = [
        ["Appetizers", groupedItems.Appetizers],
        ["Entrees", groupedItems.Entrees],
        ["Desserts", groupedItems.Desserts],
        ["Beverages", groupedItems.Beverages],
        ["Specials", groupedItems.Specials]
    ].filter(([, sectionItems]) => sectionItems.length > 0);

    return element(
        React.Fragment,
        null,
        element(
            "div",
            { className: "menu-layout" },
            element(
                "aside",
                { className: "menu-sidebar" },
                element("h2", { className: "menu-sidebar-title" }, "Filters"),
                element(
                    "div",
                    { className: "filters" },
                    element(
                        "label",
                        null,
                        "Category",
                        element(
                            "select",
                            {
                                value: filters.category,
                                onChange: (event) => updateFilter("category", event.target.value)
                            },
                            element("option", { value: "" }, "All"),
                            ...categories.map((category) =>
                                element("option", { key: category, value: category }, category))
                        )
                    ),
                    element(
                        "label",
                        null,
                        "Max Price",
                        element("input", {
                            type: "number",
                            step: "0.01",
                            placeholder: "25.00",
                            value: filters.maxPrice,
                            onChange: (event) => updateFilter("maxPrice", event.target.value)
                        })
                    ),
                    element(
                        "label",
                        null,
                        "Allergens",
                        element("input", {
                            type: "text",
                            placeholder: "gluten, dairy, nuts",
                            value: filters.allergen,
                            onChange: (event) => updateFilter("allergen", event.target.value)
                        }),
                        element(
                            "div",
                            { className: "checkbox-row form-checkbox-row" },
                            element("input", {
                                id: "exclude-allergens",
                                type: "checkbox",
                                checked: filters.excludeAllergens,
                                onChange: (event) => updateFilter("excludeAllergens", event.target.checked)
                            }),
                            element(
                                "label",
                                { className: "checkbox-label", htmlFor: "exclude-allergens" },
                                "Exclude items with these allergens"
                            )
                        )
                    )
                )
            ),
            element(
                "div",
                { className: "menu-content" },
                isLoading
                    ? element("p", { className: "status-message" }, "Loading menu...")
                    : null,
                error
                    ? element("article", { className: "error-banner" }, error)
                    : null,
                !isLoading && !error && filteredItems.length > 0
                    ? element("p", { className: "status-message" }, `${filteredItems.length} item(s) match your filters.`)
                    : null,
                !isLoading && !error && filteredItems.length === 0
                    ? element("p", { className: "status-message" }, "No items match these filters.")
                    : null,
                element(
                    "div",
                    { className: "menu-grid menu-sections" },
                    ...visibleSections.map(([title, sectionItems]) => renderMenuSection(title, sectionItems))
                )
            )
        )
    );
}

if (host) {
    const categories = JSON.parse(host.dataset.categories ?? "[]");
    const itemsUrl = host.dataset.itemsUrl ?? "/Menu/Items";
    createRoot(host).render(element(MenuApp, { categories, itemsUrl }));
}
