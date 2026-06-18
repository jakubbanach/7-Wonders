# MCTS+NN Training Pipeline

## Wymagania
- Zapis self-play z C# przez `GameConsole` w formacie `.npz`.
- `MoveLog.State`, `MoveLog.ActionMask`, `MoveLog.ActionIndex`, `MoveLog.PolicyTarget`.
- `targets/policy_targets.npy` i `targets/value_targets.npy` w pliku treningowym.
- Python: `torch`, `numpy`, `onnx`.
- C#: istniejący `self-play-train` w `GameConsole`.

## Minimalny flow
1. Uruchom self-play na modelu `.onnx`.
2. Zapisz dane treningowe do `.npz`.
3. Wczytaj dane w Pythonie.
4. Trenuj policy/value network.
5. Zapisz osobny model dla tej iteracji.
6. Użyj nowego modelu jako wejścia do kolejnej iteracji.

## Polecenie
```powershell
python GameAI/MCTS_NN_pipeline_old/train_mcts_nn_iterations.py --rounds 5 --games 1000 --epochs 20
```

## Wynik iteracji
- `GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_001.pt`
- `GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_iter_001.onnx`
- `GameAI/Encoding/onnx_models/mcts_nn_iterations/policy_network_latest.onnx`
- `GameAI/Encoding/onnx_models/mcts_nn_iterations/training_manifest.json`
