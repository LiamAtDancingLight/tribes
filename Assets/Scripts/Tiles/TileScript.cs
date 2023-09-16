using System;
using System.Collections.Generic;
using AI;
using Characters;
using Data.Actions;
using Data.GeneralTiles;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Tiles
{
    public class TileScript : NavigationNode, IInteractable
    {
        [SerializeField] private TileData _tileData;
        [SerializeField] private MeshFilter _groundMeshFilter;
        [SerializeField] private MeshRenderer _groundRenderer;
        [SerializeField] private GameObject _secondLayer;
  

        private GameObject _currentTile;
        private TileScript _townTile;

        public TileData TileData => _tileData;
        public GameObject CurrentTile => _currentTile;
        public TileScript TownTile => _townTile;
        public bool IsExplored { get; set; } = true;

        public Unit Occupant { get; private set; }

        public UnityAction<Unit> onOccupantEntered;

        public void OnClicked()
        {
            PlayClickedAnimation();

            Character selectedCharacter = SelectionManager.Instance.SelectedCharacter;
            Unit selectedUnit = SelectionManager.Instance.SelectedUnit;
            
            
            Unit chosenUnit =  selectedUnit;
            
            // If there is only one character selected, split it from its current unit, and give it a new unit
            if (!chosenUnit && selectedCharacter)
            {
                if (SelectionManager.Instance.IsTileSelected(this))
                {
                    if (!Occupant || Occupant.TeamIndex == 0)
                        selectedCharacter.CurrentUnit.SplitFromUnit(selectedCharacter);
                    
                    chosenUnit = selectedCharacter.CurrentUnit;
                }
            }

            // If there is a unit selected, move it to this tile
            if (chosenUnit)
            {
                chosenUnit.AI.SetState(AIState.None);
                
                if (SelectionManager.Instance.IsTileSelected(this))
                {
                    bool success = chosenUnit.NavigateToTile(this);
                    print(success);
                    if (success)
                    {
                        UIManager.instance.CloseMenu();
                        return;
                    }
                }
            }
            
            // Select tiles in radius
            SelectionManager.Instance.DeselectAll();
            SelectionManager.Instance.SelectTilesInRadius(GetSelectionRadius(), this);

            if (TownTile && TownTile != this && TileData.TileType != TileType.Building)
            {
                UIManager.instance.OpenBuildMenu(this);
            }
            else
            {
                UIManager.instance.CloseMenu();
            }
        }
        
        public void Interact(Character character)
        {
            if (!_currentTile || !_currentTile.TryGetComponent(out ITileInterface tileInterface)) return;
            
            tileInterface.OnInteract(character);
            PlayInteractAnimation();
        }

        public void OnSelected()
        {
            
        }

        public void OnDeselected()
        {
            
        }
        
        public void SetTown(TileScript townTile)
        {
            _townTile = townTile;
            TileManager.Instance.SetTileGround(this);
        }
        
        public void SetTileData(TileData tileData)
        {
            _tileData = tileData;
            
            if (!IsExplored) return;
            
            UpdateTile();
            RunActions(TileData.OnTilePlaced);
        }


        private void RunActions(List<ScriptableAction> actions)
        {
            foreach (ScriptableAction action in actions)
            {
                action.Execute(gameObject);
            }
        }

        public void SetExplored(bool newExplored)
        {
            if (newExplored == IsExplored) return;
            
            IsExplored = newExplored;

            if (IsExplored)
            {
               SetTileData(TileData);
            }
            else
            {
                ClearTile();
                _groundMeshFilter.mesh = TileManager.Instance.GetUnexploredMesh();
            }
        }

        private void UpdateTile()
        {
            if (!_tileData) return;
            
            ClearTile();
            
            if (_tileData.Tile)
            {
                _currentTile = Instantiate(_tileData.Tile, transform);
                _currentTile.transform.parent = _secondLayer.transform;
            }

            IsWalkable = _tileData.IsWalkable;
            NodeWeight = _tileData.MovementCost;
            TileManager.Instance.SetTileGround(this);
        }

        public void SetGround(Mesh groundType)
        {
            _groundMeshFilter.mesh = groundType;
        }

        public void ClearTile()
        {
            foreach (Transform child in _secondLayer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void SetOccupant(Unit occupant)
        {
            if (occupant)
            {
                onOccupantEntered?.Invoke(occupant);
            }
            Occupant = occupant;
        }
        public void ClearOccupant()
        {
            Occupant = null;
        }

        public int GetSelectionRadius()
        {
            if (_tileData)
            {
                return _tileData.SelectionRadius;
            }

            return 0;
        }

        public void PlayInteractAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(_secondLayer, 0.25f);
        }
        
        private void PlayClickedAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);
        }
    }
}
