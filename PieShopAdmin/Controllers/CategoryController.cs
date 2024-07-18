using Microsoft.AspNetCore.Mvc;
using PieShopAdmin.Models;
using PieShopAdmin.Models.Repositories;
using PieShopAdmin.ViewModel;

namespace PieShopAdmin.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        CategoryListViewModel model = new()
        {
            Categories = (await _categoryRepository.GetAllCategoriesAsync()).ToList()
        };
        
        return View(model);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        
        var category = await _categoryRepository.GetCategoryByIdAsync(id.Value);
        return View(category);
    }

    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add([Bind("Name,Description,DateAdded")] Category category)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Adding the category failed, please try again! Error: {ex.Message}");
        }

        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var selectedCategory = await _categoryRepository
            .GetCategoryByIdAsync(id.Value);
        return View(selectedCategory);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {e.Message}");
        }

        return View(category);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var selectedCategory = await _categoryRepository.GetCategoryByIdAsync(id);
        return View(selectedCategory);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int? categoryId)
    {
        if (categoryId == null)
        {
            ViewData["ErrorMessage"] = "Deleting the category failed, invalid ID!";
            return View();
        }

        try
        {
            await _categoryRepository.DeleteCategoryAsync(categoryId.Value);
            TempData["CategoryDeleted"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = $"Deleting the category failed, please try again! Error: {ex.Message}";
        }

        var seletedCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId.Value);
        return View(seletedCategory);
    }

    public async Task<IActionResult> BulkEdit()
    {
        List<CategoryBulkViewModel> categoryBulkViewModels = new();

        var allCategories = await _categoryRepository
            .GetAllCategoriesAsync();
        foreach (var category in allCategories)
        {
            categoryBulkViewModels.Add(new()
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            });
        }

        return View(categoryBulkViewModels);
    }

    [HttpPost]
    public async Task<IActionResult> BulkEdit(List<CategoryBulkViewModel> categoryBulkViewModels)
    {
        List<Category> categories = new();

        foreach (var categoryBulkViewModel in categoryBulkViewModels)
        {
            categories.Add(new()
            {
                CategoryId = categoryBulkViewModel.CategoryId,
                Name = categoryBulkViewModel.Name
            });
        }

        await _categoryRepository.UpdateCategoryNamesAsync(categories);
        return RedirectToAction(nameof(Index)); 
    }
}