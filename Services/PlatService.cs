using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Borne.Models;

namespace Borne.Services
{
    /// <summary>
    /// Gestion CRUD des plats.
    /// Persistance locale en JSON (AppData/Borne/plats.json).
    /// Un chargement depuis la BDD sera possible en surcharge future.
    /// </summary>
    public class PlatService
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static PlatService? _instance;
        public  static PlatService Instance => _instance ??= new PlatService();

        // ── État ───────────────────────────────────────────────────────────────
        private List<Plat> _plats = new();

        private static readonly string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Borne", "plats.json");

        private int _nextId = 1;

        // ══════════════════════════════════════════════════════════════════════
        private PlatService()
        {
            Load();
        }

        // ── Chargement ─────────────────────────────────────────────────────────
        private void Load()
        {
            if (File.Exists(DataPath))
            {
                try
                {
                    string json = File.ReadAllText(DataPath);
                    _plats  = JsonSerializer.Deserialize<List<Plat>>(json) ?? new();
                    _nextId = _plats.Count > 0 ? _plats.Max(p => p.Id) + 1 : 1;
                    return;
                }
                catch { /* fichier corrompu → on réinitialise */ }
            }

            // Données par défaut
            _plats = new List<Plat>
            {
                new(1,  "Burger Classic",     "Steak haché, salade, tomate, fromage",   12.50m, "Plat principal"),
                new(2,  "Pizza Margherita",   "Tomate, mozzarella, basilic",            11.00m, "Plat principal"),
                new(3,  "Salade César",       "Poulet grillé, croûtons, parmesan",       9.50m, "Entrée"),
                new(4,  "Frites maison",      "Portion de frites maison",                4.00m, "Accompagnement"),
                new(5,  "Tiramisu",           "Mascarpone, café, biscuits",              6.50m, "Dessert"),
                new(6,  "Boisson (33cl)",     "Coca, Fanta ou Sprite",                   3.00m, "Boisson"),
                new(7,  "Steak Frites",       "Steak de bœuf avec frites",              15.00m, "Plat principal"),
                new(8,  "Glace vanille",      "2 boules de glace vanille",               5.00m, "Dessert"),
                new(9,  "Soupe du jour",      "Voir le tableau du jour",                 6.00m, "Entrée"),
                new(10, "Eau plate (50cl)",   "Eau minérale naturelle",                  2.00m, "Boisson"),
            };
            _nextId = 11;
            Save();
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataPath)!);
                File.WriteAllText(DataPath,
                    JsonSerializer.Serialize(_plats, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* non bloquant */ }
        }

        // ── API publique ───────────────────────────────────────────────────────

        public List<Plat> GetAll() => new(_plats);

        public Plat? GetById(int id) => _plats.FirstOrDefault(p => p.Id == id);

        public void Add(Plat plat)
        {
            plat.Id = _nextId++;
            _plats.Add(plat);
            Save();
        }

        public void Update(Plat updated)
        {
            int idx = _plats.FindIndex(p => p.Id == updated.Id);
            if (idx >= 0)
            {
                _plats[idx] = updated;
                Save();
            }
        }

        public void Delete(int id)
        {
            _plats.RemoveAll(p => p.Id == id);
            Save();
        }
    }
}
